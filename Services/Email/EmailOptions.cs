using quiz_web_app.Infrastructure.Templates;

namespace quiz_web_app.Services.Email
{
    public record class EmailOptions(string subject, string to, string filepath, EmailModel model);
}
