using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services.CacheService;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using static DemoApiWithoutEF.Utilities.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagement_API.Services
{
    public class StudentServices : IStudentServices
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IJwtServices _jwtService;
        private readonly IMapper _mapper;
        private readonly ICacheServices _cacheServices;
        public StudentServices(IConfiguration configuration, IJwtServices jwtService, IMapper mapper, ICacheServices cacheServices)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _jwtService = jwtService;
            _mapper = mapper;
            _cacheServices = cacheServices;
        }

        public T GetData<T>(string query, string cacheKey)
        {
            T obj = _cacheServices.GetSingleCachedResponse<T>(cacheKey);
            if (obj == null)
            {
                T newobj = DbClient.ExecuteOneRecordProcedureWithQuery<T>(query, null);
                _cacheServices.SetSingleCachedResponse(cacheKey, newobj);
                return newobj;
            }
            return obj;
        }

        public T GetStudent<T>(string Procedure, int Id)
        {
                Collection<DbParameters> parameters = new();
                parameters.Add(new DbParameters { Name = "@StudentId", Value = Id, DBType = DbType.Int64 });
                T newobj = DbClient.ExecuteOneRecordProcedure<T>(Procedure, parameters);
                _cacheServices.SetSingleCachedResponse("Student" + Id, newobj);
                return newobj;

        }


        public IList<T> GetRecordsWithoutPagination<T>(string ProcedureName, string cacheKey)
        {
            IList<T> list = DbClient.ExecuteProcedure<T>(ProcedureName, null);
            _cacheServices.SetListCachedResponse<T>(cacheKey, list);
            return list;
        }

        public IList<T> GetDataWithPagination<T>(PaginationDto paginationDto, string cacheKey, string sp)
        {
            try
            {
                Collection<DbParameters> parameters = new();
                parameters.Add(new DbParameters { Name = "@Search_Query", Value = paginationDto.searchQuery ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Sort_Column_Name", Value = paginationDto.OrderBy ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Start_index", Value = paginationDto.StartIndex, DBType = DbType.Int64 });
                parameters.Add(new DbParameters { Name = "@Page_Size", Value = paginationDto.PageSize, DBType = DbType.Int64 });
                //IList<Book> books = DbClient.ExecuteProcedure<Book>("[dbo].[Get_Books_List]", parameters);
                IList<T> data = DbClient.ExecuteProcedure<T>(sp, parameters);

                _cacheServices.SetListCachedResponse<T>(cacheKey, data);
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query)
        {

            if (studentUpdateDto != null)
            {
                var table = new DataTable();
                table.Columns.Add("FirstName");
                table.Columns.Add("LastName");
                table.Columns.Add("BirthDate");
                table.Columns.Add("CourseId");
                table.Columns.Add("UserName");
                table.Columns.Add("Password");
                table.Columns.Add("Email");

                var row = table.NewRow();
                row["FirstName"] = studentUpdateDto.FirstName;
                row["LastName"] = studentUpdateDto.LastName;
                row["BirthDate"] = ((DateTime)studentUpdateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentUpdateDto.CourseId;
                row["UserName"] = studentUpdateDto.UserName;
                row["Password"] = studentUpdateDto.Password;
                row["Email"] = studentUpdateDto.Email;
                table.Rows.Add(row);

                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@StudentId", Value = studentUpdateDto.StudentId, DBType = DbType.Int64 });
                parameters.Add(new DbParameters() { Name = "@Student_Details", Value = table, DBType = DbType.Object, TypeName = "Student_Details" });
                DbClient.ExecuteProcedure("Update_Student_Details", parameters, ExecuteType.ExecuteNonQuery);
            }
            else
            {
                var table = new DataTable();
                table.Columns.Add("FirstName");
                table.Columns.Add("LastName");
                table.Columns.Add("BirthDate");
                table.Columns.Add("CourseId");
                table.Columns.Add("UserName");
                table.Columns.Add("Password");
                table.Columns.Add("Email");

                var row = table.NewRow();
                row["FirstName"] = studentCreateDto.FirstName;
                row["LastName"] = studentCreateDto.LastName;
                row["BirthDate"] = ((DateTime)studentCreateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentCreateDto.CourseId;
                row["UserName"] = studentCreateDto.UserName;
                row["Password"] = studentCreateDto.Password;
                row["Email"] = studentCreateDto.Email;

                table.Rows.Add(row);

                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@Student_Details", Value = table, DBType = DbType.Object, TypeName = "Student_Details" });
                DbClient.ExecuteProcedure("Add_Student_Details", parameters, ExecuteType.ExecuteNonQuery);

            }
        }

        public int AddEditScheduledEmailLogs(EmailLogs? emailLogs, string query)
        {
            var table = new DataTable();
            table.Columns.Add("StudentId");
            table.Columns.Add("Subject");
            table.Columns.Add("Body");
            table.Columns.Add("SentDate");
            table.Columns.Add("SentBy");

            var row = table.NewRow();
            row["StudentId"] = emailLogs.StudentId == 0 ? null : emailLogs.StudentId;
            row["Subject"] = emailLogs.Subject;
            row["Body"] = emailLogs.Body;
            row["SentDate"] = ((DateTime)emailLogs.SentDate).ToString("MM-dd-yyyy");
            row["SentBy"] = emailLogs.SentBy;
            table.Rows.Add(row);
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@email_details", Value = table, DBType = DbType.Object, TypeName = "Email_Details" });
            if (emailLogs.ScheduledEmailId != 0)
            {
                parameters.Add(new DbParameters() { Name = "@scheduledEmailId", Value = emailLogs.ScheduledEmailId, DBType = DbType.Int64, });
            }
            DbParameters outputParameter = new DbParameters()
            {
                Name = "@newScheduledEmailId",
                DBType = DbType.Int64,
                Direction = ParameterDirection.Output
            };
            parameters.Add(outputParameter);

            EmailLogs scheduledEmailIdLogs = DbClient.ExecuteOneRecordProcedure<EmailLogs>(query, parameters);
            return scheduledEmailIdLogs.ScheduledEmailId;
        }

        public void UpdateAttachments(EmailLogs? emailLogs, string query)
        {
            var table = new DataTable();
            table.Columns.Add("AttachmentFile", typeof(byte[]));
            table.Columns.Add("ScheduledEmailId");

            foreach (var attachment in emailLogs.AttachmentsByte)
            {
                var row = table.NewRow();
                row["AttachmentFile"] = attachment;
                row["ScheduledEmailId"] = emailLogs.ScheduledEmailId;
                table.Rows.Add(row);
            }

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailid", Value = emailLogs.ScheduledEmailId, DBType = DbType.Int64, });
            parameters.Add(new DbParameters() { Name = "@attachments", Value = table, DBType = DbType.Object, TypeName = "AttachmentsTableType" });
            DbClient.ExecuteProcedure(query, parameters, ExecuteType.ExecuteNonQuery);
        }
        public void AddEmailAttachments(Byte[] attachment,string fileName, int scheduledEmailId, string query)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailid", Value = scheduledEmailId, DBType = DbType.Int64, });
            parameters.Add(new DbParameters() { Name = "@attachment", Value = attachment, DBType = DbType.Binary, });
            parameters.Add(new DbParameters() { Name = "@fileName", Value = fileName, DBType = DbType.String, });

            DbClient.ExecuteProcedure(query, parameters, ExecuteType.ExecuteNonQuery);
        }



        public void AddEmailLogs(EmailLogs? emailLogs, string query)
        {
            var table = new DataTable();
            table.Columns.Add("Email");
            table.Columns.Add("Subject");
            table.Columns.Add("Body");
            table.Columns.Add("SentDate");
            table.Columns.Add("SentBy");
            table.Columns.Add("IsSent");


            var row = table.NewRow();
            row["Email"] = emailLogs.Email;
            row["Subject"] = emailLogs.Subject;
            row["Body"] = emailLogs.Body;
            row["SentDate"] = DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss");
            row["SentBy"] = emailLogs.SentBy;
            row["IsSent"] = emailLogs.IsSent;
            table.Rows.Add(row);

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@email_details", Value = table, DBType = DbType.Object, TypeName = "[Email_log_details]" });
            parameters.Add(new DbParameters() { Name = "@emailLogId", DBType = DbType.Int64, Direction = ParameterDirection.Output });
            DbClient.ExecuteProcedure(query, parameters, ExecuteType.ExecuteNonQuery);
        }


        public void DeleteStudent(int StudentId)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@Id", Value = StudentId, DBType = DbType.Object, TypeName = "Student_Details" });
            DbClient.ExecuteProcedureWithQuery("Delete From Students where StudentId=@Id", parameters, ExecuteType.ExecuteNonQuery);
        }

        public JwtClaimsDto GetLoginStudentDetails(StudentLoginDto studentLoginDto)
        {
            try
            {
                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@UserName", Value = studentLoginDto.UserName, DBType = DbType.String });
                parameters.Add(new DbParameters() { Name = "@PassWord", Value = studentLoginDto.Password, DBType = DbType.String });
                JwtClaimsDto jwtClaims = DbClient.ExecuteOneRecordProcedure<JwtClaimsDto>("[dbo].[Get_UserName_Password]", parameters);
                if (jwtClaims.StudentId != 0)
                {
                    jwtClaims.JwtToken = _jwtService.GenerateToken(jwtClaims);
                    UpdateJwtToken(jwtClaims.JwtToken, jwtClaims.StudentId);
                }
                return jwtClaims;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UpdateJwtToken(string jwtToken, int StudentId)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@Id", Value = StudentId, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@JwtToken", Value = jwtToken, DBType = DbType.String });
            DbClient.ExecuteProcedureWithQuery("Update Students SET JwtToken = @JwtToken Where StudentId = @Id", parameters, ExecuteType.ExecuteNonQuery);
        }
        public static T GetDataModel<T>(object dataObj)
        {
            return ((JObject)dataObj).ToObject<T>();
        }

        public dynamic GetDynamicData(string controllerName, string methodName, object dataObj)
        {
            if (controllerName == "Student" && methodName == "GetStudent")
            {
                return Convert.ToInt32(dataObj);
            }
            else if (controllerName == "Login" && methodName == "CheckLoginDetails")
            {
                return GetDataModel<StudentLoginDto>(dataObj);
            }
            else if ((controllerName == "Student" && methodName == "LoginStudentDetails") ||
                    (controllerName == "ProfessorHod" && methodName == "LoginDetails"))
            {
                return GetDataModel<StudentLoginDto>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "CreateStudent")
            {
                return GetDataModel<StudentCreateDto>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "UpdateStudent")
            {
                return GetDataModel<StudentUpdateDto>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "UpdateStudentJwtToken")
            {
                return GetDataModel<UpdateJwtDTo>(dataObj);
            }
            else if (controllerName == "ProfessorHod" && methodName == "UpdateProfessorHodJwtToken")
            {
                return GetDataModel<UpdateJwtDTo>(dataObj);
            }
            else if (controllerName == "ProfessorHod" && methodName == "SendEmail")
            {
                return GetDataModel<EmailLogs>(dataObj);
            }
            else if (controllerName == "Email" && methodName == "GetScheduledEmailById")
            {
                return Convert.ToInt32(dataObj);
            }
            else if (controllerName == "Student" && methodName == "DeleteStudent")
            {
                return Convert.ToInt32(dataObj);
            }
            else if (controllerName == "Course" && methodName == "GetCourse")
            {
                return Convert.ToInt32(dataObj);
            }
            else if ((controllerName == "Student" && methodName == "GetAllStudents")
                || (controllerName == "Book" && methodName == "GetAllBooks")
                || (controllerName == "Email" && methodName == "GetScheduledEmails"))
            {
                return GetDataModel<PaginationDto>(dataObj);
            }
            else if (controllerName == "Course" && methodName == "CreateCourse")
            {
                return GetDataModel<CourseCreateDto>(dataObj);
            }
            else if ((controllerName == "Book" && methodName == "CreateBook") || (controllerName == "Book" && methodName == "UpdateBook") ||
                (controllerName == "Book" && methodName == "DeleteBook") || (controllerName == "Book" && methodName == "GetBookPhoto"))
            {
                return GetDataModel<Book>(dataObj);
            }
            else if ((controllerName == "Book" && methodName == "GetBook"))
            {
                return Convert.ToInt32(dataObj);
            }
            else if ((controllerName == "Email" && methodName == "AddEditScheduledEmailLogs")
                || (controllerName == "Email" && methodName == "AddEmailLogs") ||
                (controllerName == "Student" && methodName == "GetEmailFromStudentId")
                || (controllerName == "Email" && methodName == "GetDayWiseEmailCount"))
            {
                return GetDataModel<EmailLogs>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "DayWiseCountStudentProf")
            {
                return GetDataModel<CountStudentProfessorDto>(dataObj);
            }
            else
            {
                return null;
            }
        }

        public void InsertCourse(CourseCreateDto? courseCreateDto, string query)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@CourseName", Value = courseCreateDto.CourseName, DBType = DbType.String });
            DbClient.ExecuteProcedureWithQuery("Insert into Courses(CourseName) values (@CourseName)", parameters, ExecuteType.ExecuteNonQuery);
        }

        public void InsertBook(Book book)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@bookTitle", Value = book.BookTitle, DBType = DbType.String });
            parameters.Add(new DbParameters() { Name = "@courseId", Value = book.CourseId, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@subject", Value = book.Subject, DBType = DbType.String });
            if (book.PhotoName != null)
            {
                parameters.Add(new DbParameters() { Name = "@photoName", Value = book.PhotoName ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters() { Name = "@photo", Value = book.Photo, DBType = DbType.Binary, });

            }
            DbClient.ExecuteProcedure("[dbo].[add_edit_books]", parameters, ExecuteType.ExecuteNonQuery);
        }

        public void UpdateBook(Book book)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@bookTitle", Value = book.BookTitle, DBType = DbType.String });
            parameters.Add(new DbParameters() { Name = "@courseId", Value = book.CourseId, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@subject", Value = book.Subject, DBType = DbType.String });
            parameters.Add(new DbParameters() { Name = "@bookId", Value = book.BookId, DBType = DbType.Int64 });
            if (book.PhotoName != null)
            {
                parameters.Add(new DbParameters() { Name = "@photoName", Value = book.PhotoName ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters() { Name = "@photo", Value = book.Photo, DBType = DbType.Binary, });
            }
            DbClient.ExecuteProcedure("[dbo].[add_edit_books]", parameters, ExecuteType.ExecuteNonQuery);
        }

        public void DeleteBook(int BookId)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@bookId", Value = BookId, DBType = DbType.Int64 });
            DbClient.ExecuteProcedure("[dbo].[delete_book]", parameters, ExecuteType.ExecuteNonQuery);
        }

        public Book GetBookPhoto(int BookId)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@bookId", Value = BookId, DBType = DbType.Int64 });
            Book book = DbClient.ExecuteOneRecordProcedure<Book>("[dbo].[Get_BookPhoto]", parameters);
            return book;
        }

        public IList<EmailLogs> GetEmailsAndStudentIds()
        {
            IList<EmailLogs> emailLogs = DbClient.ExecuteProcedure<EmailLogs>("[dbo].[Get_Student_Id_Email]", null);
            return emailLogs;
        }
        public T GetScheduledEmailById<T>(string Procedure, int Id)
        {
            Collection<DbParameters> parameters = new();
            parameters.Add(new DbParameters { Name = "@scheduledEmailId", Value = Id, DBType = DbType.Int64 });
            T obj = DbClient.ExecuteOneRecordProcedure<T>(Procedure, parameters);
            return obj;
        }


        public IList<EmailLogs> GetDayWiseEmailCount(EmailLogs emailLog)
        {
            Collection<DbParameters> parameters = new();
            parameters.Add(new DbParameters { Name = "@month", Value = emailLog.month, DBType = DbType.Int64 });
            parameters.Add(new DbParameters { Name = "@year", Value = emailLog.year, DBType = DbType.Int64 });

            IList<EmailLogs> emailLogs = DbClient.ExecuteProcedure<EmailLogs>("[dbo].[get_daywise_count_email]", parameters);
            return emailLogs;
        }

        public IList<CountStudentProfessorDto> GetDayWiseProfStudentCount(CountStudentProfessorDto countStudentProfessorDto)
        {
            Collection<DbParameters> parameters = new();
            parameters.Add(new DbParameters { Name = "@month", Value = countStudentProfessorDto.month, DBType = DbType.Int64 });
            parameters.Add(new DbParameters { Name = "@year", Value = countStudentProfessorDto.year, DBType = DbType.Int64 });

            IList<CountStudentProfessorDto> list = DbClient.ExecuteProcedure<CountStudentProfessorDto>("[dbo].[Get_StudentProfessor_Count_byDate]", parameters);
            return list;
        }

        public bool IsPDF(byte[] bytes)
        {
            byte[] PDFSignature = { 37, 80, 68, 70, 45, 49, 46 };
            if (bytes.Length >= PDFSignature.Length &&
         bytes.Take(PDFSignature.Length).SequenceEqual(PDFSignature))
            {
                return true;
            }
            return false;
        }

        public IList<EmailLogs> GetAttachementsFromScheduledId(int scheduledId)
        {
            string Sql = "[dbo].[Get_Attachment_By_ScheduledEmailId]";

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailid", Value = scheduledId, DBType = DbType.Int64 });
            return DbClient.ExecuteProcedure<EmailLogs>(Sql, parameters);
        }

        public SettingDto GetApiVersion()
        {
            string query = "SELECT SettingDescription FROM Settings Where SettingName = 'ApiVersion'";
            return DbClient.ExecuteOneRecordProcedureWithQuery<SettingDto>(query, null);
        }
    }
}
