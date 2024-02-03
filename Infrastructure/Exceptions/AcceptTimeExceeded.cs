namespace quiz_web_app.Infrastructure.Exceptions
{
    public class AcceptTimeExceeded : BaseQuizAppException
    {
        public AcceptTimeExceeded(string errorMessage) : base(errorMessage)
        {
        }
    }
}
