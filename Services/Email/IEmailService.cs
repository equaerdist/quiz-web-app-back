namespace quiz_web_app.Services.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailOptions options);
    }
}
