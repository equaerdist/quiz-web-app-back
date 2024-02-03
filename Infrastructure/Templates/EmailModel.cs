using quiz_web_app.Services.Email;

namespace quiz_web_app.Infrastructure.Templates
{
    public class EmailModel
    {
        public RequestInfo RequestInfo { get; set; }
        public string Href { get; set; }
        public EmailModel(RequestInfo info, string href) 
        {
            RequestInfo = info;
            Href = href;
        }
    }
}
