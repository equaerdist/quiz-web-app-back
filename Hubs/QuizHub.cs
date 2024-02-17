using Amazon.S3.Model;
using Core.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Models;
using RedLockNet.SERedis;

namespace quiz_web_app.Hubs
{
    [Authorize]
    public class QuizHub : Hub<IClient>
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly RedLockFactory _redLock;
        private readonly TimeSpan _inviteAvailableTime = TimeSpan.FromMinutes(5);
        private static readonly string _twoPeopleQueue = "queue_quiz_2";
        private static readonly string _threePeopleQueue = "queue_quiz_3";
        private static readonly string _fourPeopleQueue = "queue_quiz_4";
        private static readonly TimeSpan _lockTime = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _wait = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan _retry = TimeSpan.FromSeconds(1);
        public QuizHub(QuizAppContext ctx, IDistributedCache cache, RedLockFactory redLock) 
        {
            _ctx = ctx;
            _cache = cache;
            _redLock = redLock;
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
        private async Task HandleGroupOfRandoms(EnterQueueInfo info)
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
                await HandleQueue(parameters);
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
        public async Task GoToQueue(EnterQueueInfo info)
        {
            if (info.PeopleAmount < 1)
                throw new ArgumentException(nameof(info.PeopleAmount));
            if (info.PeopleAmount > 1 && info.CompetitiveType != CompetitiveType.Multi)
                throw new HubException();
            if (!info.WithGroup)
            {
                if (info.PeopleAmount != 1)
                    await HandleGroupOfRandoms(info);
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
                    await HandleQueue(param);
                }
            }
            else
            {
                var user = await _ctx.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.Id.Equals(Context.UserIdentifier));
                if (user is null)
                    throw new HubException();
                var usersExceptCurrent = user.Group?.Members.Select(u => u.Id).Where(u => !u.Equals(user.Id)).ToList();
                if (usersExceptCurrent is null)
                    throw new HubException();
                var param = new QueueParameters()
                {
                    Info = info,
                    CurrentUser = user.Id,
                    Queue = new() { Users = usersExceptCurrent },
                    CurrentQueue = string.Empty
                };
                await HandleQueue(param);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
