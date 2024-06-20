using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;

namespace StudentManagement_API.Services
{
    public interface IProfessorHodServices
    {
        ProfessorHod CheckUserNamePassword(StudentLoginDto studentLoginDto);

        bool IsAuthorized(ApiRequest apiRequest);

        void UpdateJwtToken(string jwtToken, int Id);
    }
}
