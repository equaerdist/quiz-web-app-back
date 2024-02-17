﻿using Amazon.S3.Model;
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
            IQuizRepository quizes) 
        {
            _ctx = ctx;
            _cache = cache;
            _redLock = redLock;
            _cfg = cfg;
            _mapper = mapper;
            _quizes = quizes;
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
                            StartTime = DateTime.UtcNow
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
        public async Task CheckMyQuestion(CheckAnswerInfo info)
        {
            var answerInfo = new AnswerInfo();
            var userSessionQuizInfo = await _cache.GetStringAsync(Context.UserIdentifier!);
            if (userSessionQuizInfo is null)
                throw new HubException();
            var userSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(userSessionQuizInfo)!;
            var quiz = await _quizes.GetByIdAsync(userSession.Result.QuizId);
        }
        private async Task SendAnswerToUser(Guid userId)
        {
            var userQuizSession = await _cache.GetStringAsync(userId.ToString());
            if (userQuizSession is null)
                throw new ArgumentNullException(nameof(userQuizSession));
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(userQuizSession)!;
            var completed = quizSession.Result;

            var quiz = await _quizes.GetByIdAsync(completed.QuizId);
            var quizDto = _mapper.Map<GetQuizDto>(quiz);
            if (quizDto.QuestionsAmount == completed.Answers.Count)
                await UserCompleteQuiz();
            else
            {
                var quizCard = quizDto.QuizCards.Skip(completed.Answers.Count).Take(1).First();
                await Clients.User(userId.ToString()).ReceiveQuestion(quizCard);
            }
        }
        private async Task UserCompleteQuiz()
        {
            throw new NotImplementedException();
        }
        public async Task GoToQueue(EnterQueueInfo info)
        {
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
                    var redisQueue = new RedisQueue() { Users = new() { currentUser } };
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
                sessions.Select(u => u.Result.UserId).ForEach(async id => await SendAnswerToUser(id));
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}