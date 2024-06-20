

using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using System.Data;

namespace StudentManagement_API.Services
{
    public interface IStudentServices
    {
        T GetData<T>(string query);

        T GetStudent<T>(string Procedure, int Id);

        IList<Student> GetDataWithPegination(PaginationDto paginationDto);

        IList<Book> GetBooksWithPegination(PaginationDto paginationDto);

        void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query);

        void DeleteStudent(int StudentId);

        Student GetLoginStudentDetails(StudentLoginDto studentLoginDto);

        void UpdateJwtToken(string jwtToken, int StudentId);

        dynamic GetDynamicData(string controllerName, string methodName, object dataObj);

        void InsertCourse(CourseCreateDto? courseCreateDto, string query);

        IList<T> GetRecordsWithoutPagination<T>(string ProcedureName);

        void InsertBook(Book book);

        void UpdateBook(Book book);
    }
}
