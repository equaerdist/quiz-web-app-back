using Core.Models;

namespace quiz_web_app.Hubs
{
    public class UserQuizSessionInfo
    {
        public Completed Result { get; set; } = null!;
        public Guid QuizSessionInfoId { get; set; }
    }
}
