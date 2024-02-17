using Amazon.S3.Model;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Models;

namespace quiz_web_app.Hubs
{
    [Authorize]
    public class QuizHub : Hub<IClient>
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _inviteAvailableTime = TimeSpan.FromMinutes(5);
        public QuizHub(QuizAppContext ctx, IDistributedCache cache) 
        {
            _ctx = ctx;
            _cache = cache;
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
        public Task GoToQueue(EnterQueueInfo info)
        {
            if(!info.WithGroup)
            {
                _cache
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
