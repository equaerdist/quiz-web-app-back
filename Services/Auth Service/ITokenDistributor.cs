using System.Security.Claims;

namespace quiz_web_app.Services.Auth_Service
{
    public interface ITokenDistributor
    {
        string GetToken(List<Claim>? claims);
    }
}
