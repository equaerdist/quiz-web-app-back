using quiz_web_app.Services.Email;

namespace quiz_web_app.Infrastructure.Consumers.UserRegisteredEventConsumer
{
    public class UserRegisteredEvent
    {
        public EmailOptions EmailOptions { get; set; } = null!;
    }
}
