using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;

namespace StudentManagment.Services.Interface
{
    public interface IBaseServices
    {
        Student CheckLoginDetails(StudentViewModel studentViewModel);

        bool UpdateJwtToken(string token, int StudentId);

        Student GetStudentDetailById(int id,string token);

        Course GetCourseDetailById(int id);

        Student GetStudentByMaster(int id, string token);
    }
}
