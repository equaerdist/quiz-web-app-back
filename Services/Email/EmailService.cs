using FluentEmail.Core;
using Internal;

namespace quiz_web_app.Services.Email
{
    public class EmailService : IEmailService
    {

        private IFluentEmail _fluentEmail;

        public EmailService(IFluentEmail fluentEmail)
        {
            _fluentEmail = fluentEmail;
        }


        public async Task SendAsync(EmailOptions options)
        {
            var email = _fluentEmail
                            .To(options.to)
                            .Subject(options.subject)
                            .UsingTemplateFromFile(options.filepath, options.model);
            await email.SendAsync();

        }
    }
}
