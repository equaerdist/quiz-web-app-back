using quiz_web_app.Infrastructure;

namespace quiz_web_app.Services.KeyResolver
{
    public class KeyResolver : IKeyResolver
    {
        private readonly AppConfig _cfg;
        private readonly string _threePeopleQueue;
        private readonly string _twoPeopleQueue;
        private readonly string _fourPeopleQueue;

        public KeyResolver(AppConfig cfg)
        {
            _cfg = cfg;
            #region локальные биндинги
            _threePeopleQueue = _cfg.ThreePeopleQueue;
            _twoPeopleQueue = _cfg.TwoPeopleQueue;
            _fourPeopleQueue = _cfg.FourPeopleQueue;
            #endregion
        }

        public string GetGroupSessionKey(Guid sessionId) =>
            $"{_cfg.GroupSessionPrefix}_{sessionId}";

        public string GetOrderedQuizCardsKey(Guid quizId)
            => $"{_cfg.QuizCardCachePrefix}_{quizId}";

        public string GetQuizKey(Guid quizId) =>
            $"{_cfg.QuizCachePrefix}_{quizId}";

        public string GetQuizQueueLock(int peopleAmount, Guid quizId)
        {
            var currentLock = peopleAmount == 2 ? _twoPeopleQueue :
            peopleAmount == 3 ? _threePeopleQueue : _fourPeopleQueue;
            currentLock += quizId;
            return currentLock;
        }

        public string GetUserMatchResultKey(Guid userId, Guid QuizId) =>
            $"{_cfg.MatchEndsCachePrefix}_{userId}_{QuizId}";
      

        public string GetUserSessionKey(Guid userId, Guid QuizId) =>
            $"{_cfg.UserSessionPrefix}_{userId}_{QuizId}";
    }
}
