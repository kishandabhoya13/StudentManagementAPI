using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;

namespace StudentManagement_API.Services.Interface
{
    public interface IProfessorHodServices
    {
        ProfessorHod CheckUserNamePassword(StudentLoginDto studentLoginDto);

        bool IsAuthorized(ApiRequest apiRequest);

        void UpdateJwtToken(string jwtToken, int Id);
    }
}
