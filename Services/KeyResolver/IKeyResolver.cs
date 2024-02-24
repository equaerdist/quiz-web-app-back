namespace quiz_web_app.Services.KeyResolver
{
    public interface IKeyResolver
    {
        string GetOrderedQuizCardsKey(Guid quizId);
        string GetUserSessionKey(Guid userId, Guid quizId);
        string GetUserMatchResultKey(Guid userId, Guid quizId);
        string GetQuizKey(Guid quizId);
        string GetGroupSessionKey(Guid sessionId);
        string GetQuizQueueLock(int peopleAmount, Guid quizId);
     
    }
}
