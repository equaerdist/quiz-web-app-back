using Amazon.S3.Model;
using AutoMapper;
using Core.Models;
using FluentEmail.Core;
using Internal;
using MassTransit;
using MassTransit.Logging;
using MassTransit.NewIdProviders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Consumers.UserCompleteQuizEventConsumer;
using quiz_web_app.Models;
using quiz_web_app.Services.KeyResolver;
using quiz_web_app.Services.Repositories.QuizRepository;
using RedLockNet.SERedis;
using static quiz_web_app.Hubs.Button;

namespace quiz_web_app.Hubs
{
    [Authorize]
    public class QuizHub : Hub<IClient>
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly RedLockFactory _redLock;
        private readonly AppConfig _cfg;
        private readonly IMapper _mapper;
        private readonly IQuizRepository _quizes;
        private readonly ILogger<QuizHub> _logger;
        private readonly IBus _bus;
        private readonly IKeyResolver _keyResolver;
        private readonly TimeSpan _inviteAvailableTime = TimeSpan.FromMinutes(5);
        private static readonly string _startInfo = "queue_information";
        private static readonly TimeSpan _lockTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _wait = TimeSpan.FromHours(1);
        private static readonly TimeSpan _retry = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _cancelTime = TimeSpan.FromSeconds(1);
        public QuizHub(QuizAppContext ctx, 
            IDistributedCache cache, 
            RedLockFactory redLock,
            AppConfig cfg,
            IMapper mapper,
            IQuizRepository quizes,
            ILogger<QuizHub> logger,
            IBus bus,
            IKeyResolver keyResolver) 
        {
            #region инициализация сервисов
            _ctx = ctx;
            _cache = cache;
            _redLock = redLock;
            _cfg = cfg;
            _mapper = mapper;
            _quizes = quizes;
            _logger = logger;
            _bus = bus;
            _keyResolver = keyResolver;
            #endregion
           
        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }


        private async Task UserNotExists()
        {
            var message = new Message()
            {
                Content = "Пользователя с таким логином не существует",
                Type = MessageType.Error,
                NotifyType = NotifyType.Main
            };
            await Clients.Caller.ReceiveMessage(message);
        }


        public async Task InviteMates(Guid id)
        {
            var user = await _ctx.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (user == null)
            {
                await UserNotExists();
                return;
            }
            if(user.Group is not null)
            {
                var message = new Message()
                {
                    Content = $"Пользователь  с  id = {id} уже состоит в группе", 
                    Type = MessageType.Error,
                    NotifyType = NotifyType.Main
                };
                await Clients.Caller.ReceiveMessage(message);
                return;
            }
            var inviteMessage = new Message() 
            { 
                Content = $"Вас пригласил в группу пользователь {Context.UserIdentifier}",
                Type = MessageType.Error,
                NotifyType = NotifyType.Main,
                Buttons = new() 
                { 
                    new() { 
                            Content = "Принять",
                            Action = "AcceptInvite",
                            TransferInfo = Context.UserIdentifier
                        }
                }
            };
            var key = $"{Context.UserIdentifier}_{id}";
            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = _inviteAvailableTime
            };
            await _cache.SetStringAsync(key, string.Empty, cacheOptions);
            await Clients.User(user.Id.ToString()).ReceiveMessage(inviteMessage);
        }
        
        public async Task AcceptInvite(Guid id) 
        {
            var inviterUser = await _ctx.Users.Include(u => u.Group)
                .ThenInclude(g => g.Members)
                .FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (inviterUser is null)
            {
                await UserNotExists();
                return;
            }
            var cacheInvite = await _cache.GetStringAsync($"{id}_{Context.UserIdentifier}");
            if(cacheInvite is null)
            {
                var message = new Message()
                {
                    Content = $"Приглашение пользователя {id} истекло",
                    Type = MessageType.Error,
                    NotifyType = NotifyType.Main,
                };
                await Clients.Caller.ReceiveMessage(message);
                return;
            }
            if(inviterUser.Group is null)
            {
                var message = new Message()
                {
                    Content = "Такой группы уже не существует",
                    Type = MessageType.Error
                };
                await Clients.Caller.ReceiveMessage(message);
                return;
            }
            if(inviterUser.Group.Playing)
            {
                var message = new Message()
                {
                    Content = "Вы не можете присоедниниться к группе, которая играет",
                    Type = MessageType.Error,
                };
                await Clients.Caller.ReceiveMessage(message);
                return;
            }
            var currentUser = await _ctx.Users
                .FirstOrDefaultAsync(u => u.Id.Equals(Context.UserIdentifier));
            if (currentUser is null)
                throw new ArgumentNullException(nameof(currentUser));
            currentUser.GroupId = inviterUser.GroupId;
            await _ctx.SaveChangesAsync();
            var invitedMessage = new Message()
            {
                Content = "Вы были добавлены в группу",
                AdditionActions = AdditionActions.ResetGroupInfo
            };
            var inviterMessage = new Message()
            {
                Content = $"Пользователь {currentUser.Id} принял ваше приглашение",
                AdditionActions = AdditionActions.ResetGroupInfo
            };
            await Clients.User(currentUser.Id.ToString()).ReceiveMessage(invitedMessage);
            await Clients.User(inviterUser.Id.ToString()).ReceiveMessage(inviterMessage);
        }

        public async Task CancelQueue()
        {
            
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                return;
            _logger.LogInformation($"Вызвана отмена поиска для {userId}");
            if (!(Context.Items[_startInfo] is QueueStatus status))
                return;
            var queueKey = _keyResolver.GetQuizQueueLock(status.EnterQueueInfo.PeopleAmount, status.EnterQueueInfo.QuizId);
            if (status.Token is not null)
                status.Token.Cancel();
            using var redisLock = await _redLock.CreateLockAsync(queueKey, _lockTime, _cancelTime, _retry);
            if (!redisLock.IsAcquired)
                return;
            var cachedQueue = await _cache.GetStringAsync(queueKey);
            if (cachedQueue is null)
                return;
           
            var queue = JsonConvert.DeserializeObject<RedisQueue>(cachedQueue)!;
            queue.Users.Remove(userId);
            await _cache.SetStringAsync(queueKey, JsonConvert.SerializeObject(queue));
        }
        private async Task<List<UserQuizSessionInfo>?> HandleGroupOfRandoms(EnterQueueInfo info, CancellationToken token = default)
        {
            var currentLock = _keyResolver.GetQuizQueueLock(info.PeopleAmount, info.QuizId);
            try
            {
                using var redlock = await _redLock.CreateLockAsync(currentLock,
                    _lockTime, _wait, _retry, token);

                if (!redlock.IsAcquired)
                    throw new Exception();

                var queue = await _cache.GetStringAsync(currentLock);
                if (!Guid.TryParse(Context.UserIdentifier, out var currentUser))
                    throw new HubException();

                if (queue is null)
                {
                    var redisQueue = new RedisQueue() { Users = new() { currentUser } };
                    queue = JsonConvert.SerializeObject(redisQueue);
                    await _cache.SetStringAsync(currentLock, queue);
                    return null;
                }
                else
                {
                    var redisQueue = JsonConvert.DeserializeObject<RedisQueue>(queue)!;
                    var parameters = new QueueParameters()
                    {
                        Info = info,
                        Queue = redisQueue,
                        CurrentQueue = currentLock,
                        CurrentUser = currentUser
                    };
                    var sessions = await HandleQueue(parameters);
                    if (sessions is not null)
                    {
                        var emptyQueue = new RedisQueue() { Users = new() };
                        await _cache.SetStringAsync(currentLock,
                            JsonConvert.SerializeObject(emptyQueue));
                    }
                    return sessions;
                }
            }
            catch(OperationCanceledException)
            {
                return null;
            }
        }


        private async Task<List<UserQuizSessionInfo>?> HandleQueue(QueueParameters p)
        {
            p.Queue.Users.Add(p.CurrentUser);
            if (p.Queue.Users.Count == p.Info.PeopleAmount)
            {
                var session = p.Info.CompetitiveType == CompetitiveType.Multi ?
                    new QuizSessionInfo() { Users = p.Queue.Users } : null;
                List<UserQuizSessionInfo> userResults = new();
                foreach (var user in p.Queue.Users)
                {
                    var userResult = new UserQuizSessionInfo()
                    {
                        QuizSessionInfoId = session?.Id,
                        Result = new Completed()
                        {
                            QuizId = p.Info.QuizId,
                            CompetitiveType = p.Info.CompetitiveType,
                            UserId = user,
                            Fulfilled = false,
                            Score = 0,
                        }
                    };
                    await _cache.SetStringAsync(user.ToString(), 
                        JsonConvert.SerializeObject(userResult));
                    userResults.Add(userResult);
                }
                if (session is not null)
                {
                    var sessionKey = _keyResolver.GetGroupSessionKey(session.Id);
                    await _cache.SetStringAsync(sessionKey,
                        JsonConvert.SerializeObject(session));
                }
                return userResults;
            }
            else
            {
                await _cache.SetStringAsync(p.CurrentQueue, JsonConvert.SerializeObject(p.Queue));
                return null;
            }
        }

        public async Task<AnswerInfo> CheckMyQuestion(CheckAnswerInfo info)
        {
            if (info.Answers is null)
                throw new HubException();
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                throw new HubException();
            var quizSessionCache = await _cache.GetStringAsync(userId.ToString());
            if (quizSessionCache is null)
                throw new HubException();
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(quizSessionCache)!;
            var currentAnswer = quizSession.Result.Answers.Last();
            var currentQuiz = await _quizes.GetByIdAsync(quizSession.Result.QuizId);
            var answersForCheck = currentQuiz.QuizCards
                .First(c => c.Id.Equals(currentAnswer.CardId))
                .QuestionsRelationships;
            var awardForSingleCard = currentQuiz.Award / currentQuiz.QuizCards.Count;
            var amountOfRightAnswers = 0;
            var amountOfRightByUser = 0;
            var rightAnswerdIds = new List<Guid>();
            foreach(var question in answersForCheck)
            {
                if (question.Type)
                {
                    amountOfRightAnswers++;
                    rightAnswerdIds.Add(question.QuestionId);
                }
                if(info.Answers.Contains(question.QuestionId) && question.Type)
                    amountOfRightByUser++;
            }
            var awardForRightAnswer = (int)Math.Round((double)awardForSingleCard * 
                amountOfRightByUser / amountOfRightAnswers);
            currentAnswer.Type = !(awardForRightAnswer != awardForSingleCard);
            var elapsed = DateTime.UtcNow - currentAnswer.StartTime;
            currentAnswer.Elapsed = elapsed;
            quizSession.Result.Score += awardForRightAnswer;
            var answerInfo = new AnswerInfo()
            { 
                Award = awardForRightAnswer,
                Elapsed = elapsed,
                RightAnswers = rightAnswerdIds
            };
            if(quizSession.Result.Answers.Count == currentQuiz.QuestionsAmount)
            {
                var elapsedFinally = DateTime.UtcNow - quizSession.Result.StartTime;
                if (quizSession.Result.Score > currentQuiz.Award)
                    quizSession.Result.Score = currentQuiz.Award;
                quizSession.Result.Elapsed = elapsedFinally;
                quizSession.Result.Fulfilled = true;
                var matchEndsInfo = new MatchEndsInfo()
                {
                    Elapsed = elapsedFinally,
                    Score = quizSession.Result.Score,
                    AmountOfRightAnswers = quizSession.Result.Answers.Count(a => a.Type)
                };
                var endKey = _keyResolver.GetUserMatchResultKey(userId, quizSession.Result.QuizId);
                await _cache.SetStringAsync(endKey, JsonConvert.SerializeObject(matchEndsInfo));
                var message = new UserCompleteQuizEvent()
                {
                    UserId = userId,
                    QuizId = quizSession.Result.QuizId
                };
                var completeQuizKey = _keyResolver.GetUserSessionKey(userId, quizSession.Result.QuizId);
                await _cache.RemoveAsync(userId.ToString());
                await _cache.SetStringAsync(completeQuizKey, JsonConvert.SerializeObject(quizSession));
                await _bus.Publish(message);
            }
            else
                await _cache.SetStringAsync(userId.ToString(), JsonConvert.SerializeObject(quizSession));
            return answerInfo;
        }

        public async Task<MatchEndsInfo?> GetInformationAboutQuizCompletion(Guid quizId)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                throw new HubException();
            var endKey = _keyResolver.GetUserMatchResultKey(userId, quizId);
            var resultCache = await _cache.GetStringAsync(endKey);
            if (resultCache is null)
                return null;
            var matchResult = JsonConvert.DeserializeObject<MatchEndsInfo>(resultCache);
            return matchResult;
        }

        public async Task<GetQuizCardDto> SendAnswerToUser()
        {
            var userId = Guid.Parse(Context.UserIdentifier ?? throw new HubException());
            var userQuizSession = await _cache.GetStringAsync(userId.ToString());
            if (userQuizSession is null)
                throw new ArgumentNullException(nameof(userQuizSession));
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(userQuizSession)!;
            var completed = quizSession.Result;

            var quiz = await _quizes.GetByIdAsync(completed.QuizId);
            var quizDto = _mapper.Map<GetQuizDto>(quiz);

            //Так как карточки могут подгружаться из бд через include они приходят в разном порядке,
            //поэтому важно каждый раз сортировать их, чтобы не терять порядка
            var key = _keyResolver.GetOrderedQuizCardsKey(quizDto.Id);
            var quizesCache = await _cache.GetStringAsync(key);
            if(quizesCache is null)
            {
                var orderedQuizCards = quizDto.QuizCards.OrderBy(c => c.Name);
                await _cache.SetStringAsync(key, JsonConvert.SerializeObject(
                    new CacheWrapper<IEnumerable<GetQuizCardDto>>() { Data = orderedQuizCards})
                );
                quizesCache = await _cache.GetStringAsync(key);
            }
            var quizes = JsonConvert.DeserializeObject<CacheWrapper<IEnumerable<GetQuizCardDto>>>
            (quizesCache ?? throw new HubException())!;
            var quizCard = quizes.Data.Skip(completed.Answers.Count).Take(1).First();
            quizCard.Award = (int)Math.Round((double)quizDto.Award / quizDto.QuestionsAmount);
            if (completed.Answers.Count == 0)
                quizSession.Result.StartTime = DateTime.UtcNow;
            var answer = new CardAnswer()
            {
                StartTime = DateTime.UtcNow,
                CardId = quizCard.Id,
                Completed = completed,
            };
            completed.Answers.Add(answer);
            userQuizSession = JsonConvert.SerializeObject(quizSession);
            await _cache.SetStringAsync(userId.ToString(), userQuizSession);
            return quizCard;
        }


        public async Task GoToQueue(EnterQueueInfo info)
        {
            _logger.LogInformation($"Пришла заявку на очередь от {Context.UserIdentifier}");
            var queueEntryInformation = new QueueStatus() { EnterQueueInfo = info, Token = info.PeopleAmount != 1 ? new () : null };
            Context.Items[_startInfo] = queueEntryInformation;
            if (info.PeopleAmount < 1)
                throw new ArgumentException(nameof(info.PeopleAmount));
            if (info.PeopleAmount > 1 && info.CompetitiveType != CompetitiveType.Multi)
                throw new HubException();
            List<UserQuizSessionInfo>? sessions = null;
            if (!info.WithGroup)
            {
                if (info.PeopleAmount != 1)
                    sessions = await HandleGroupOfRandoms(info, queueEntryInformation.Token!.Token);
                else
                {
                    if (!Guid.TryParse(Context.UserIdentifier, out var currentUser))
                        throw new HubException();
                    var redisQueue = new RedisQueue() { Users = new()};
                    var param = new QueueParameters()
                    {
                        Info = info,
                        CurrentQueue = string.Empty,
                        CurrentUser = currentUser,
                        Queue = redisQueue
                    };
                    sessions = await HandleQueue(param);
                }
            }
            else
            {
                var user = await _ctx.Users.Include(u => u.Group)
                    .FirstOrDefaultAsync(u => u.Id.Equals(Context.UserIdentifier));
                if (user is null)
                    throw new HubException();
                var usersExceptCurrent = user.Group?.Members
                    .Select(u => u.Id).Where(u => !u.Equals(user.Id)).ToList();
                using var redLock = await _redLock
                    .CreateLockAsync($"GroupLocks_{user.GroupId}", 
                    _lockTime);
                if (!redLock.IsAcquired)
                {
                    var message = new Message()
                    { 
                        Type = MessageType.Error, 
                        Content = "Кто-то уже начал игру в группе"
                    };

                    await Clients.Caller.ReceiveMessage(message);
                    return;
                }
                if (usersExceptCurrent is null)
                    throw new HubException();
                var param = new QueueParameters()
                {
                    Info = info,
                    CurrentUser = user.Id,
                    Queue = new() { Users = usersExceptCurrent },
                    CurrentQueue = string.Empty
                };
                sessions = await HandleQueue(param);
            }
            if (sessions is not null)
            {
                var quiz = await _quizes.GetByIdAsync(info.QuizId);
                var usersIds = sessions.Select(u => u.Result.UserId);
                var matchInfo = new MatchStartsInfo()
                {
                    Users = usersIds,
                    CompetitiveType = sessions.First().Result.CompetitiveType,
                    QuizId = sessions.First().Result.QuizId,
                    AmountOfQuestion = quiz.QuestionsAmount
                };
                if (info.PeopleAmount != 1)
                {
                    var status = Context.Items[_startInfo] as QueueStatus;
                    if (status is null)
                        throw new ArgumentNullException();
                    status.Token?.Dispose();
                    status.Token = null;
                }
                _logger.LogInformation($"начал игру для окружения пользователя {Context.UserIdentifier}");
                await Clients.Users(usersIds.Select(id => id.ToString())).GameStarts(matchInfo);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
