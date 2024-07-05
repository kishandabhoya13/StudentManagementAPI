using StudentManagement_API.Models;
using StudentManagement_API.Models.Models.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace StudentManagement_API.Services
{
    public interface IJwtServices
    {
        string GenerateToken(JwtClaimsDto jwtClaims);

        bool ValidateToken(string? token, out JwtSecurityToken jwtSecurityToken);
    }
}
