using Microsoft.AspNetCore.Mvc;
using respNewsV8.Models;
using respNewsV8.Services;

namespace respNewsV8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UserController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            if (!_userService.IsValidUser(user))
            {
                return Unauthorized(new { Message = "Invalid username or password" });
            }

            var tokenString = _jwtService.GenerateJwtToken(user.UserName);
            return Ok(new { Token = tokenString });
        }
    }
}
