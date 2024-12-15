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
        private readonly RespNewContext _sql;


        public UserController(IUserService userService, IJwtService jwtService, RespNewContext sql)
        {
            _userService = userService;
            _jwtService = jwtService;
            _sql = sql; 
        }


        [HttpGet("users")]
        public IActionResult Get()
        {
            var users=_sql.Users.ToList();
            return Ok(users);
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            try
            {
                if (login == null || string.IsNullOrEmpty(login.UserName) || string.IsNullOrEmpty(login.UserPassword))
                {
                    return BadRequest(new { Message = "Username and password are required" });
                }

                // Kullanıcı doğrulama
                var user = _sql.Users.FirstOrDefault(u => u.UserName == login.UserName);
                if (user == null || !_userService.IsValidUser(new User { UserName = login.UserName, UserPassword = login.UserPassword }))
                {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }

                // JWT Token üretimi
                var tokenString = _jwtService.GenerateJwtToken(user.UserName);
                return Ok(new { Token = tokenString });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Error = ex.Message });
            }
        }




    }
}
