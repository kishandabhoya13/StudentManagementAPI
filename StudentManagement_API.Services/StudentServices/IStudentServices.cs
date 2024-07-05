﻿

using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using System.Data;

namespace StudentManagement_API.Services
{
    public interface IStudentServices
    {
        T GetData<T>(string query,string cacheKey);

        T GetStudent<T>(string Procedure, int Id);

        //IList<Student> GetDataWithPegination(PaginationDto paginationDto);

        //IList<Book> GetBooksWithPegination(PaginationDto paginationDto);

        void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query);

        void DeleteStudent(int StudentId);

        JwtClaimsDto GetLoginStudentDetails(StudentLoginDto studentLoginDto);

        void UpdateJwtToken(string jwtToken, int StudentId);

        dynamic GetDynamicData(string controllerName, string methodName, object dataObj);

        void InsertCourse(CourseCreateDto? courseCreateDto, string query);

        IList<T> GetRecordsWithoutPagination<T>(string ProcedureName,string cacheKey);

        void InsertBook(Book book);

        void UpdateBook(Book book);

        void DeleteBook(int BookId);

        Book GetBookPhoto(int BookId);

        IList<EmailLogs> GetEmailsAndStudentIds();

        void AddEditScheduledEmailLogs(EmailLogs? emailLogs, string query);

        void AddEmailLogs(EmailLogs? emailLogs, string query);

        //IList<EmailLogs> GetScheduledEmailsWithPegination(PaginationDto paginationDto);

        IList<T> GetDataWithPagination<T>(PaginationDto paginationDto, string cacheKey, string sp);

        T GetScheduledEmailById<T>(string Procedure, int Id);

        IList<EmailLogs> GetDayWiseEmailCount(EmailLogs emailLogs);

        IList<CountStudentProfessorDto> GetDayWiseProfStudentCount(CountStudentProfessorDto countStudentProfessorDto);
    }
}
