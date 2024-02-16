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

        public AuthController(ILogger<AuthController> logger,
            ITokenDistributor tdk,
            IValidator<UserDto> validator,
            QuizAppContext ctx,
            IMapper mapper,
            IHasher hasher,
            IBus bus)
        {
            _logger = logger;
            _tdk = tdk;
            _validator = validator;
            _ctx = ctx;
            _mapper = mapper;
            _hasher = hasher;
            _bus = bus;
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

        [HttpPost("token")]
        public async Task<IActionResult> GetToken(UserDto information)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Login.Equals(information.Login)).ConfigureAwait(false);
            if (user is null)
                throw new BaseQuizAppException("Такого пользователя не существует");
            var passwordConfirm = await _hasher.VerifyHashAsync(information.Password, user.Password).ConfigureAwait(false);
            if (passwordConfirm is false)
                throw new BaseQuizAppException("Проверьте правильность введенных данных");
            var claims = new List<Claim>()
            { 
                new Claim("Confirmed", user.Accepted ? "True" : "False"),
                new Claim("Login", user.Login)
            };
            var token = _tdk.GetToken(claims);
            Response.Cookies.Append("Authorization", token, new() { Expires = DateTime.Now.AddDays(1), HttpOnly = true });
            return Ok();
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
        [Authorize]
        [HttpGet("check")]
        public IActionResult CheckAuth() => Ok();
    }
}
