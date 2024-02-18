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
using quiz_web_app.Models;
using quiz_web_app.Services.Repositories.QuizRepository;
using RedLockNet.SERedis;

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
        private readonly TimeSpan _inviteAvailableTime = TimeSpan.FromMinutes(5);
        private static readonly string _twoPeopleQueue = "queue_quiz_2";
        private static readonly string _threePeopleQueue = "queue_quiz_3";
        private static readonly string _fourPeopleQueue = "queue_quiz_4";
        private static readonly TimeSpan _lockTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _wait = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan _retry = TimeSpan.FromSeconds(1);
        public QuizHub(QuizAppContext ctx, 
            IDistributedCache cache, 
            RedLockFactory redLock,
            AppConfig cfg,
            IMapper mapper,
            IQuizRepository quizes,
            ILogger<QuizHub> logger) 
        {
            _ctx = ctx;
            _cache = cache;
            _redLock = redLock;
            _cfg = cfg;
            _mapper = mapper;
            _quizes = quizes;
            _logger = logger;
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
                NotifyType = NotifyType.Main
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
                    NotifyType = NotifyType.Main
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


        private async Task<List<UserQuizSessionInfo>?> HandleGroupOfRandoms(EnterQueueInfo info)
        {
            var currentLock = info.PeopleAmount == 2 ? _twoPeopleQueue :
            info.PeopleAmount == 3 ? _threePeopleQueue : _fourPeopleQueue;

            using var redlock = await _redLock.CreateLockAsync(currentLock, _lockTime, _wait, _retry);

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
                return await HandleQueue(parameters);
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
                            UserId = p.CurrentUser,
                            Fulfilled = false,
                            Score = 0,
                        }
                    };
                    await _cache.SetStringAsync(user.ToString(), JsonConvert.SerializeObject(userResult));
                    userResults.Add(userResult);
                }
                if (session is not null)
                    await _cache.SetStringAsync(session.Id.ToString(), JsonConvert.SerializeObject(session));
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
            var userId = Context.UserIdentifier ?? throw new HubException();
            var quizSessionCache = await _cache.GetStringAsync(userId.ToString());
            if (quizSessionCache is null)
                throw new HubException();
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(quizSessionCache)!;
            var currentAnswer = quizSession.Result.Answers.Last();
            var currentQuiz = await _quizes.GetByIdAsync(quizSession.Result.QuizId);
            var answersForCheck = currentQuiz.QuizCards.First(c => c.Id.Equals(currentAnswer.CardId))
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
            var awardForRightAnswer = (int)Math.Round((double)awardForSingleCard * amountOfRightByUser / amountOfRightAnswers);
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
            await _cache.SetStringAsync(userId.ToString(), JsonConvert.SerializeObject(quizSession));
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
                var endKey = $"{Context.UserIdentifier}_{currentQuiz.Id}";
                await _cache.SetStringAsync(endKey, JsonConvert.SerializeObject(matchEndsInfo));
            }
            return answerInfo;
        }

        public async Task<MatchEndsInfo?> GetInformationAboutQuizCompletion(Guid quizId)
        {
            var endKey = $"{Context.UserIdentifier}_{quizId}";
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
            var key = _cfg.QuizCardCachePrefix + quizDto.Id.ToString();
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
            if (info.PeopleAmount < 1)
                throw new ArgumentException(nameof(info.PeopleAmount));
            if (info.PeopleAmount > 1 && info.CompetitiveType != CompetitiveType.Multi)
                throw new HubException();
            List<UserQuizSessionInfo>? sessions = null;
            if (!info.WithGroup)
            {
                if (info.PeopleAmount != 1)
                    sessions = await HandleGroupOfRandoms(info);
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
                var user = await _ctx.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id.Equals(Context.UserIdentifier));
                if (user is null)
                    throw new HubException();
                var usersExceptCurrent = user.Group?.Members.Select(u => u.Id).Where(u => !u.Equals(user.Id)).ToList();
                using var redLock = await _redLock.CreateLockAsync(user.GroupId.ToString(), _lockTime);
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
                await Clients.Users(usersIds.Select(id => id.ToString())).GameStarts(matchInfo);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
