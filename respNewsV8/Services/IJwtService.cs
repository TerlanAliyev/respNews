using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using respNewsV8.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace respNewsV8.Services
{
    public interface IJwtService
    {
        string GenerateJwtToken(string username);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly RespNewContext _sql;

        public JwtService(IConfiguration configuration, RespNewContext sql)
        {
            _configuration = configuration;
            _sql = sql;  // DbContext'i buraya ekliyoruz
        }

        public string GenerateJwtToken(string username)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // Kullanıcıyı veritabanından al
                var user = _sql.Users.FirstOrDefault(u => u.UserName == username);
                if (user == null) throw new UnauthorizedAccessException("User not found");

                var claims = new[]
                {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.UserRole)  // Rolü JWT'ye ekliyoruz
            };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Token generation failed", ex);
            }
        }
    }


}
