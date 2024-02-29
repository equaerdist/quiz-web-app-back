using AutoMapper;
using FluentEmail.Core;
using FluentValidation;
using Internal;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Consumers.UserRegisteredEventConsumer;
using quiz_web_app.Infrastructure.Exceptions;
using quiz_web_app.Infrastructure.Templates;
using quiz_web_app.Models;
using quiz_web_app.Services.Auth_Service;
using quiz_web_app.Services.Email;
using quiz_web_app.Services.Hasher;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;

namespace quiz_web_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        /// <summary>
        /// Определяет количетсво итераций (12 / 4,096 iterations)
        /// Это правильно только для реализации BCrypt.Net
        /// </summary>
        private readonly int _cost = 12;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenDistributor _tdk;
        private readonly IValidator<UserDto> _validator;
        private readonly QuizAppContext _ctx;
        private readonly IMapper _mapper;
        private readonly IHasher _hasher;
        private readonly IBus _bus;
        private readonly AppConfig _cfg;

        public AuthController(ILogger<AuthController> logger,
            ITokenDistributor tdk,
            IValidator<UserDto> validator,
            QuizAppContext ctx,
            IMapper mapper,
            IHasher hasher,
            IBus bus,
            AppConfig cfg)
        {
            _logger = logger;
            _tdk = tdk;
            _validator = validator;
            _ctx = ctx;
            _mapper = mapper;
            _hasher = hasher;
            _bus = bus;
            _cfg = cfg;
        }
        [HttpPost("register")]
        public async Task<IActionResult> GetRegisterAsync(UserDto information)
        {
            await _validator.ValidateAndThrowAsync(information);
            var dbUser = await _ctx.Users.FirstOrDefaultAsync(u => u.Login == information.Login).ConfigureAwait(false);
            if (dbUser is not null)
                throw new UserExistsAlreadyException("Пользователь с таким email уже зарегистрирован");
            var hashedPasswordTask = _hasher
                .GetHashAsync(information.Password, _cost)
                .ConfigureAwait(false);
            var userForDb = _mapper.Map<User>(information);
            userForDb.Thumbnail = "default.png";
            userForDb.Password = await hashedPasswordTask;
            await _ctx.Users.AddAsync(userForDb);
            await _ctx.SaveChangesAsync();
            var pathForAccept = $"{Request.Scheme}://{Request.Host}/api/auth/accept?id={userForDb.Id}";
            var emailOptions = new EmailOptions
                (
                    "Подтверждение аккаунта",
                    information.Login,
                    Path.Combine(Environment.CurrentDirectory, "Infrastructure\\Templates\\Email.cshtml"),
                    new EmailModel
                        (
                            new RequestInfo
                            (
                                Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                                Request.Headers.UserAgent
                            ),
                            pathForAccept
                        )
                );
            await _bus.Publish(new UserRegisteredEvent() { EmailOptions = emailOptions });
            return Ok("Подтвердите почту для активации аккаунта");
        }
        private List<Claim>? GetUserClaims(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim("Confirmed", user.Accepted ? "True" : "False"),
                new Claim("Login", user.Login),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            return claims;
        }
        [HttpPost()]
        public async Task<IActionResult> GetAuthentification(UserDto information)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Login.Equals(information.Login))
                .ConfigureAwait(false);
            if (user is null)
                throw new BaseQuizAppException("Такого пользователя не существует");
            var passwordConfirm = await _hasher.VerifyHashAsync(information.Password, user.Password).ConfigureAwait(false);
            if (passwordConfirm is false)
                throw new BaseQuizAppException("Проверьте правильность введенных данных");
            var claims = GetUserClaims(user);
            var token = _tdk.GetToken(claims);
            var refreshToken = string.Empty;
            if (user.RefreshToken is null || user.RefreshToken.Expires < DateTime.UtcNow)
            {
                var newRefreshToken = _tdk.GetRefreshToken();
                refreshToken = newRefreshToken.Token;
                user.RefreshToken = newRefreshToken;
                await _ctx.SaveChangesAsync();
            }
            else
                refreshToken = user.RefreshToken.Token;
            var authData = new AuthData()
            {
                Token = token.token,
                Expires = token.ExpiresTime,
                RefreshToken = refreshToken
            };
            return Ok(authData);
        }
        [HttpGet("accept")]
        public async Task<IActionResult> AcceptAccount(Guid id)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
            if (user is null) throw new AcceptTimeExceeded($"Время ожидания превысило заданный период");
            user.Accepted = true;
            await _ctx.SaveChangesAsync();
            return Ok($"Аккаунт для пользователя {user.Login} успешно активирован");
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            string? refreshToken = Request.Headers[_cfg.RefreshAlias];
            string? jwt = Request.Headers[_cfg.AuthorizationAlias];
            if (refreshToken is null || jwt is null)
                throw new BaseQuizAppException("Refresh токен не был найден");
            var userId = Guid.Parse(await _tdk.GetUserIdentifier(jwt));
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId));
            if (user is null || user.RefreshToken is null)
                throw new BaseQuizAppException("Авторизации для пользователя не было проведено");
            var userRefreshToken = user.RefreshToken;
            if (userRefreshToken.Token != refreshToken || userRefreshToken.Expires < DateTime.UtcNow)
                throw new BaseQuizAppException("Refresh token оказался невалидным. Требуется повторная аутентификация");
            var claims = GetUserClaims(user);
            var jwtTemporaryToken = _tdk.GetToken(claims);
            AuthData data = new()
            {
                Token = jwtTemporaryToken.token,
                Expires = jwtTemporaryToken.ExpiresTime
            };
            return Ok(data);
            
        }
    }
}
