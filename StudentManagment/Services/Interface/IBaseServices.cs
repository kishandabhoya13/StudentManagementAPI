using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;

namespace StudentManagment.Services.Interface
{
    public interface IBaseServices
    {
        Student CheckLoginDetails(StudentViewModel studentViewModel);

        ProfessorHod CheckAdminLoginDetails(AdminViewModel adminViewModel);

        bool UpdateJwtToken(string token, int StudentId, string currentToken);

        //Student GetStudentDetailById(int id,string token);

        Course GetCourseDetailById(int id);

        Student GetStudentByMaster(int id, string token);

        List<Course> GetAllCourses(string token);

        bool UpsertStudent(StudentViewModel studentViewModel);

        RoleBaseResponse GetAllStudents(string token);
    }
}
