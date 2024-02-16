using AutoMapper;
using Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quiz_web_app.Data;

namespace quiz_web_app.Controllers
{
    [ApiController()]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly QuizAppContext _ctx;
        private readonly IMapper _mapper;

        public UserController(QuizAppContext ctx,
            IMapper mapper) 
        { 
            _ctx = ctx; 
            _mapper = mapper; 
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo()
        {
            var login = User.Claims.First(c => c.Type.Equals("Login")).Value;
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Login.Equals(login)).ConfigureAwait(false);
            var userDto = _mapper.Map<GetUserDto>(user);
            return Ok(userDto);
        }
    }
}
