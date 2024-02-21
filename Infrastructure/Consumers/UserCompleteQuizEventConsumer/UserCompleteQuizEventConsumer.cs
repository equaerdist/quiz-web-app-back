using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Hubs;
using quiz_web_app.Services.KeyResolver;

namespace quiz_web_app.Infrastructure.Consumers.UserCompleteQuizEventConsumer
{
    public class UserCompleteQuizEventConsumer : IConsumer<UserCompleteQuizEvent>
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly IKeyResolver _keyResolver;

        public UserCompleteQuizEventConsumer(QuizAppContext ctx, 
            IDistributedCache cache, 
            IKeyResolver keyResolver)
        {
            _ctx = ctx;
            _cache = cache;
            _keyResolver = keyResolver;
        }
        public async Task Consume(ConsumeContext<UserCompleteQuizEvent> context)
        {
            var key = _keyResolver.GetUserSessionKey(context.Message.UserId, context.Message.QuizId);
            var sessionCache = await _cache.GetStringAsync(key);
            if (sessionCache is null)
                return;
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(sessionCache)!;
            await _cache.RemoveAsync(key);
            await _ctx.CompletedQuizes.AddAsync(quizSession.Result);
            await _ctx.SaveChangesAsync();
        }
    }
}
