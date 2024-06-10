using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace StudentManagment_API.Services.Interface
{
    public interface IJwtService
    {
        string GenerateToken(JwtClaims jwtClaims);

        bool ValidateToken(string? token, out JwtSecurityToken jwtSecurityToken);
    }
}
