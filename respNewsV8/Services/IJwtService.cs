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
        }

        public string GenerateJwtToken(string username)
        {

            var user = _sql.Users.SingleOrDefault(x => x.UserName == username);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, user.UserRole), // Kullanıcı rolü ekleniyor
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
