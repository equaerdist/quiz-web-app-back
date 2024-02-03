using Microsoft.IdentityModel.Tokens;
using quiz_web_app.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace quiz_web_app.Services.Auth_Service
{
    public class TokenDistributor : ITokenDistributor
    {
        private readonly AppConfig _cfg;

        public TokenDistributor(AppConfig cfg) 
        {
            _cfg = cfg;
        }
        public string GetToken(List<Claim>? claims)
        {
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Key)), 
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_cfg.Issuer, null, claims, null, DateTime.Now.AddDays(1), credentials);
            var handler = new JwtSecurityTokenHandler();
            var stringToken = handler.WriteToken(token);
            return stringToken;
        }
    }
}
