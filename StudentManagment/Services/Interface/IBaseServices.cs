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

        Course GetCourseDetailById(int id,int RoleId);

        Student GetStudentByMaster(int id, string token,SecondApiRequest secondApiRequest);

        IList<Course> GetAllCourses(string token,int RoleId);

        bool UpsertStudent(StudentViewModel studentViewModel);

        RoleBaseResponse<Student> GetAllStudents(string token,int RoleId);

        RoleBaseResponse<Student> GetAllStudentsWithPagination(SecondApiRequest secondApi);

        RoleBaseResponse<Book> GetAllBooksWithPagination(SecondApiRequest secondApi);

        bool InsertCouse(Course course,int RoleId);

        Task<bool> UpsertBook(BookViewModel bookViewModel);

        bool UpdateProfessorHodJwtToken(string token, int Id, string currentToken);
    }
}
