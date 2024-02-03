using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using quiz_web_app.Data;

namespace quiz_web_app.Hubs
{
    [Authorize(Policy = "confirmed")]
    public class QuizHub : Hub
    {
        private readonly QuizAppContext _ctx;

        public QuizHub(QuizAppContext ctx) 
        {
            _ctx = ctx;
        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public  Task InitailizeAsync()
        {
            return Task.CompletedTask;
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
