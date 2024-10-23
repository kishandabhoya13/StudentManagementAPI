

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

        T GetOneRecordFromId<T>(string Procedure, int Id);

        //IList<Student> GetDataWithPegination(PaginationDto paginationDto);

        //IList<Book> GetBooksWithPegination(PaginationDto paginationDto);

        void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query);

        void DeleteStudent(int StudentId);

        JwtClaimsDto GetLoginStudentDetails(StudentLoginDto studentLoginDto);

        void UpdateJwtToken(string jwtToken, int StudentId);

        dynamic GetDynamicData(string controllerName, string methodName, object dataObj);

        void InsertCourse(CourseCreateDto? courseCreateDto, string query);

        IList<T> GetRecordsWithoutPagination<T>(string ProcedureName);

        void InsertBook(Book book);

        void UpdateBook(Book book);

        void DeleteBook(int BookId);

        Book GetBookPhoto(int BookId);

        IList<EmailLogs> GetEmailsAndStudentIds();

        int AddEditScheduledEmailLogs(EmailLogs? emailLogs, string query);

        void UpdateAttachments(EmailLogs? emailLogs, string query);

        void AddEmailAttachments(Byte[] attachment,string fileName, int scheduledEmailId, string query);

        void AddEmailLogs(EmailLogs? emailLogs, string query);

        //IList<EmailLogs> GetScheduledEmailsWithPegination(PaginationDto paginationDto);

        IList<T> GetDataWithPagination<T>(PaginationDto paginationDto,string sp);

        IList<Student> GetFromToDateStudents<Student>(PaginationDto paginationDto, string sp);

        T GetScheduledEmailById<T>(string Procedure, int Id);

        IList<EmailLogs> GetDayWiseEmailCount(EmailLogs emailLogs);

        IList<CountStudentProfessorDto> GetDayWiseProfStudentCount(CountStudentProfessorDto countStudentProfessorDto);

        bool IsPDF(byte[] bytes);

        IList<EmailLogs> GetAttachementsFromScheduledId(int scheduledId);

        SettingDto GetApiVersion();

        ProfessorHod ProfessorBlockUnblockDetails(int userId);

        Student StudentBlockUnblockDetails(int userId);

        bool AprroveRejectRequest(StudentUpdateDto studentUpdateDto);

        ExchangeRate getExchangeRate(ExchangeRate exchangeRate);

        void AddExchangeRates(ExchangeRate exchangeRate);

        ExchangeRate GetExchangeRateDetails(ExchangeRate exchangeRate);

        CurrencyPairDto GetCurrencyPairData(string currencyPair);

        void UpsertRateAlert(CurrencyPairDto currencyPairDto);

        IList<CurrencyPairDto> GetRateAlerts(int StudentId);

        CurrencyPairDto GetRateAlertById(int RateAlertId);

        void RemoveRateAlert(int RateAlertId);

        void AddQueries(QueriesDto queriesDto);

        QueriesDto GetQueryDetails(int QueryId);

        RecordsCountDto GetRecordsCounts(int Id);

        IList<StudentListCountFromDateDto> GetStudentsCountFromDates(StudentListCountFromDateDto studentListCountFromDateDto);

        void AddBulkStudents(ExportExcelStudentDTO exportExcelStudentDTO);

        IList<Student> CheckUsenameList(ExportExcelStudentDTO exportExcelStudentDTO);

        ForgotPasswordDTO CheckExistingUserNamePassword(string uEmail);

        void ChangePasswordByEmail(ForgotPasswordDTO forgotPasswordDTO);

        Student CheckPasswordByStudentId(ForgotPasswordDTO forgotPasswordDTO);

        void ChangePasswordById(ForgotPasswordDTO forgotPasswordDTO);

        IList<ForgotPasswordDTO> CheckPreviousPasswords(ForgotPasswordDTO forgotPasswordDTO);

        Student GetStudentIdByEmail(ForgotPasswordDTO forgotPasswordDTO);

        void UpsertBlogs(Blog blog);

        void DeleteBlog(int blogId);
    }
}
