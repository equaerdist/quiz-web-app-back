using quiz_web_app.Infrastructure;

namespace quiz_web_app.Services.KeyResolver
{
    public class KeyResolver : IKeyResolver
    {
        private readonly AppConfig _cfg;

        public KeyResolver(AppConfig cfg)
        {
            _cfg = cfg;
        }

        public string GetGroupSessionKey(Guid sessionId) =>
            $"{_cfg.GroupSessionPrefix}_{sessionId}";

        public string GetOrderedQuizCardsKey(Guid quizId)
            => $"${_cfg.QuizCardCachePrefix}_{quizId}";

        public string GetQuizKey(Guid quizId) =>
            $"{_cfg.QuizCachePrefix}_{quizId}";

        public string GetUserMatchResultKey(Guid userId, Guid QuizId) =>
            $"{_cfg.MatchEndsCachePrefix}_{userId}_{QuizId}";
      

        public string GetUserSessionKey(Guid userId, Guid QuizId) =>
            $"{_cfg.UserSessionPrefix}_{userId}_{QuizId}";
    }
}
