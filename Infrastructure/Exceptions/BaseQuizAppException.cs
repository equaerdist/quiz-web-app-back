namespace quiz_web_app.Infrastructure.Exceptions
{
    public class BaseQuizAppException : Exception
    {
        public BaseQuizAppException(string errorMessage) : base(errorMessage) { }
    }
}
