using quiz_web_app.Models;

namespace quiz_web_app.Hubs
{
    public class RedisQueue
    {
        public List<User> Users { get; set; } = null!;
    }
}
