using StudentManagement_API.Models;
using System.IdentityModel.Tokens.Jwt;

namespace StudentManagment_API.Services.Interface
{
    public interface IJwtService
    {
        string GenerateToken(Student studentViewModel);

        bool ValidateToken(string? token, out JwtSecurityToken jwtSecurityToken);
    }
}
