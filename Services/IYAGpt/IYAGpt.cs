namespace quiz_web_app.Services.IYAGpt
{
    public interface IYAGpt
    {
        Task<string> GetCategoryAsync(string text);
    }
}
