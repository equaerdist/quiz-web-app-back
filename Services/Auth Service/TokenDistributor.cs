using Core.Models;
using Microsoft.IdentityModel.Tokens;
using quiz_web_app.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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


        public (string token, DateTime ExpiresTime) GetToken(List<Claim>? claims)
        {
            var credentials = new SigningCredentials(
                GetKey(), 
                SecurityAlgorithms.HmacSha256);
            var expiresTime = DateTime.UtcNow.Add(_cfg.TokenExpiresTime);
            var token = new JwtSecurityToken(_cfg.Issuer, null, claims, null, expiresTime, credentials);
            var handler = new JwtSecurityTokenHandler();
            var stringToken = handler.WriteToken(token);
            return (stringToken, expiresTime);
        }
        private SymmetricSecurityKey GetKey() => new(Encoding.UTF8.GetBytes(_cfg.Key));

        public RefreshToken GetRefreshToken()
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(_cfg.AmountOfKeyBytes)),
                Expires = DateTime.UtcNow.Add(_cfg.RefreshTokenExpiresTime),
                Created = DateTime.UtcNow
            };
            return refreshToken;
        }

        public async Task<string> GetUserIdentifier(string jwt)
        {
            var jwtHanlder = new JwtSecurityTokenHandler();
            var options = new TokenValidationParameters();
            options.IssuerSigningKey = GetKey();
            options.ValidIssuer = _cfg.Issuer;
            var jwtTokenValidated = await jwtHanlder.ValidateTokenAsync(jwt, options);
            var idClaim = jwtTokenValidated.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!;
            return idClaim.Value;
        }
    }
}
