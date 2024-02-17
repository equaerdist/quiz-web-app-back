using quiz_web_app.Models;

namespace quiz_web_app.Hubs
{
    public class QueueParameters
    {
        public EnterQueueInfo Info { get; set; } = null!;
        public string CurrentQueue { get; set; } = null!;
        public RedisQueue Queue { get; set; } = null!;
        public User CurrentUser { get; set; } = null!;
    }
}
