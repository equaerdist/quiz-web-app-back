namespace quiz_web_app.Services.Hasher
{
    public interface IHasher
    {
        Task<string> GetHashAsync(string key, int cost, CancellationToken token = default);
        Task<bool> VerifyHashAsync(string test, string hash);
    }
}
