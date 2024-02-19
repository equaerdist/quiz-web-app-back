using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Hubs;

namespace quiz_web_app.Infrastructure.Consumers.UserCompleteQuizEventConsumer
{
    public class UserCompleteQuizEventConsumer : IConsumer<UserCompleteQuizEvent>
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;

        public UserCompleteQuizEventConsumer(QuizAppContext ctx, IDistributedCache cache)
        {
            _ctx = ctx;
            _cache = cache;
        }
        public async Task Consume(ConsumeContext<UserCompleteQuizEvent> context)
        {
            var key = context.Message.UserId;
            var sessionCache = await _cache.GetStringAsync(key.ToString());
            if (sessionCache is null)
                return;
            var quizSession = JsonConvert.DeserializeObject<UserQuizSessionInfo>(sessionCache)!;
            await _ctx.CompletedQuizes.AddAsync(quizSession.Result);
            await _ctx.SaveChangesAsync();
        }
    }
}
