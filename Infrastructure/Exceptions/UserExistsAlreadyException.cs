namespace quiz_web_app.Infrastructure.Exceptions
{
    public class UserExistsAlreadyException : BaseQuizAppException
    {
        public UserExistsAlreadyException(string error) : base(error) { }
    }
}
