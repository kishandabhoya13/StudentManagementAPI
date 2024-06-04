using Microsoft.IdentityModel.Tokens;
using StudentManagement_API.Models;
using StudentManagment_API.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentManagment_API.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Student studentViewModel)
        {
            try
            {
                DateTime expirationTime = DateTime.UtcNow.AddHours(2);
                long unixTimestamp = ((DateTimeOffset)expirationTime).ToUnixTimeSeconds();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier,studentViewModel.FirstName + " "+ studentViewModel.LastName),
                    new Claim(ClaimTypes.Name,studentViewModel.FirstName + " "+ studentViewModel.LastName),
                    new Claim("StudentId", studentViewModel.StudentId.ToString()),
                    new Claim(ClaimTypes.Email,studentViewModel.UserName),
                    new Claim(ClaimTypes.Expiration, unixTimestamp.ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddHours(2);

                var token = new JwtSecurityToken(
                        _configuration["Jwt:Issuer"],
                        _configuration["Jwt:Audience"],
                        claims,
                        expires: expires,
                        signingCredentials: credential
                  );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool ValidateToken(string? token,out JwtSecurityToken jwtSecurityToken)
        {
            jwtSecurityToken = null;
            if(token.IsNullOrEmpty())
            {
                return false;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                },out SecurityToken validateToken);

                jwtSecurityToken = (JwtSecurityToken)validateToken;
                if(jwtSecurityToken == null)
                {
                    return false;
                }
                return true;
            }catch
            {
                return false;
            }
        }
    }
}
