using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;

namespace StudentManagment.Services.Interface
{
    public interface IBaseServices
    {
        JwtClaimsViewModel CheckLoginDetails(AdminStudentViewModel adminStudentViewModel);

        //ProfessorHod CheckAdminLoginDetails(AdminStudentViewModel adminViewModel);

        bool UpdateJwtToken(string token, int StudentId, string currentToken);

        //Student GetStudentDetailById(int id,string token);

        Course GetCourseDetailById(int id,int RoleId);

        Student GetStudentByMaster(int id, string token,SecondApiRequest secondApiRequest);

        Book GetBook(int BookId, string token, SecondApiRequest apiRequest);

        IList<Course> GetAllCourses(string token,int RoleId);

        bool UpsertStudent(StudentViewModel studentViewModel);

        RoleBaseResponse<Student> GetAllStudents(string token,int RoleId);

        RoleBaseResponse<Student> GetAllStudentsWithPagination(SecondApiRequest secondApi);

        RoleBaseResponse<Book> GetAllBooksWithPagination(SecondApiRequest secondApi);

        bool InsertCouse(Course course,int RoleId);

        Task<bool> UpsertBook(BookViewModel bookViewModel);

        bool UpdateProfessorHodJwtToken(string token, int Id, string currentToken);

        bool DeleteBook(BookViewModel bookViewModel);

        Book GetBookPhoto(BookViewModel bookViewModel);

        List<StudentsEmailAndIds> GetEmailsAndIds(int? RoleId, string JwtToken);

        EmailViewModel GetScheduledEmailById(int? RoleId, string JwtToken, int ScheduledEmailId);

        void SendEmail(EmailViewModel emailViewModel);

        string GetEmailFromStudentId(EmailViewModel emailViewModel);

        RoleBaseResponse<ScheduledEmailViewModel> GetAllScheduledEmail(SecondApiRequest secondApi);

        void UpdateScheduledEmailLog(EmailViewModel emailViewModel);

        RoleBaseResponse<CountEmailViewModel> GetDayWiseEmailCount(int month,int year, int roleId, string jwtToken);

        RoleBaseResponse<CountStudentProfessor> GetDayWiseProfStudentCount(int month, int year, int roleId, string jwtToken);
    }
}
