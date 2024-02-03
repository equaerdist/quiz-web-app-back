
using BCrypt.Net;

namespace quiz_web_app.Services.Hasher
{
    public class Hasher : IHasher
    {
        public async Task<string> GetHashAsync(string key, int cost, CancellationToken token = default)
        {
            var result = await Task.Factory.StartNew(() => BCrypt.Net.BCrypt.HashPassword(key, workFactor: cost),
                token);
            return result;
        }

        public async Task<bool> VerifyHashAsync(string test, string hash)
        {
            var result = await Task.Factory.StartNew(() => BCrypt.Net.BCrypt.Verify(test, hash));
            return result;
        }

    }
}
