using quiz_web_app.Models;

namespace quiz_web_app.Hubs
{
    public class QuizSessionInfo
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<Guid> Users { get; set; } = null!;
    }
}
