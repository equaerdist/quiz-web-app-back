using Core.Models;

namespace quiz_web_app.Hubs
{
    public class MatchStartsInfo
    {
        public List<Guid> Users { get; set; } = null!;
        public Guid QuizId { get; set; }
        public CompetitiveType CompetitiveType { get; set; }
    }
}
