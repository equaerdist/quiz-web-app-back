
using MassTransit;
using quiz_web_app.Services.Email;

namespace quiz_web_app.Infrastructure.Consumers.UserRegisteredEventConsumer
{
    public class UserRegisteredEventConsumer : IConsumer<UserRegisteredEvent>
    {
        private readonly IEmailService _email;
        private readonly ILogger<UserRegisteredEventConsumer> _logger;

        public UserRegisteredEventConsumer(IEmailService email, ILogger<UserRegisteredEventConsumer> logger) 
        {
            _email = email;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
        {
            _logger.LogInformation($"Get task for send email message for {context.Message.EmailOptions.to}");
            var emailOptions = context.Message.EmailOptions;
            await _email.SendAsync(emailOptions);
            _logger.LogInformation($"Email for {context.Message.EmailOptions.to} sent");
        }
    }
}
