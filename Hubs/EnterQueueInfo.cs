using Core.Models;

namespace quiz_web_app.Hubs
{
    public class EnterQueueInfo
    {
        public Guid QuizId { get; set; }
        public CompetitiveType CompetitiveType { get; set; }
        public int PeopleAmount { get; set; } 
        public bool WithGroup { get; set; } 
    }
}
