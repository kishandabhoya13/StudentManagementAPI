

using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using System.Data;

namespace StudentManagement_API.Services.Interface
{
    public interface IStudentServices
    {
        DataTable GetData(string query);

        void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query);

        void DeleteStudent(int StudentId);

        Student GetLoginStudentDetails(StudentLoginDto studentLoginDto);

        void UpdateJwtToken(string jwtToken, int StudentId);
    }
}
