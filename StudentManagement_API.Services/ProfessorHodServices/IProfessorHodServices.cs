using StudentManagement_API.Models.Models.DTO;

namespace StudentManagement_API.Services
{
    public interface IProfessorHodServices
    {
        JwtClaimsDto CheckUserNamePassword(StudentLoginDto studentLoginDto);

        bool IsAuthorized(ApiRequest apiRequest);

        void UpdateJwtToken(string jwtToken, int Id);
    }
}
