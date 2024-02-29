using Core.Models;
using System.Security.Claims;

namespace quiz_web_app.Services.Auth_Service
{
    public interface ITokenDistributor
    {
        (string token, DateTime ExpiresTime) GetToken(List<Claim>? claims);
        RefreshToken GetRefreshToken();
        Task<string> GetUserIdentifier(string jwt);
    }
}
