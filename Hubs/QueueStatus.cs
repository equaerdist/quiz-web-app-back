namespace quiz_web_app.Hubs
{
    public class QueueStatus
    {
        public CancellationTokenSource? Token { get; set; }
        public EnterQueueInfo EnterQueueInfo { get; set; } = null!;
    }
}
