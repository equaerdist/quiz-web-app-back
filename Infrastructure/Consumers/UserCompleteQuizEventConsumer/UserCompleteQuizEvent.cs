namespace quiz_web_app.Infrastructure.Consumers.UserCompleteQuizEventConsumer
{
    public class UserCompleteQuizEvent
    {
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
    }
}
