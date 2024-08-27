using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.SqlServer.Server;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services.CacheService;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlTypes;
using System.Net.Mail;
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
        private readonly HttpClient _httpClient;
        public StudentServices(IConfiguration configuration,
            IJwtServices jwtService, IMapper mapper, ICacheServices cacheServices, HttpClient httpClient)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _jwtService = jwtService;
            _mapper = mapper;
            _cacheServices = cacheServices;
            _httpClient = httpClient;
        }

        public T GetData<T>(string query)
        {
            T newobj = DbClient.ExecuteOneRecordProcedureWithQuery<T>(query, null);
            return newobj;
        }

        public T GetStudent<T>(string Procedure, int Id)
        {
            Collection<DbParameters> parameters = new();
            parameters.Add(new DbParameters { Name = "@StudentId", Value = Id, DBType = DbType.Int64 });
            T newobj = DbClient.ExecuteOneRecordProcedure<T>(Procedure, parameters);
            _cacheServices.SetSingleCachedResponse("Student" + Id, newobj);
            return newobj;

        }


        public IList<T> GetRecordsWithoutPagination<T>(string ProcedureName)
        {
            IList<T> list = DbClient.ExecuteProcedure<T>(ProcedureName, null);
            return list;
        }

        public IList<T> GetDataWithPagination<T>(PaginationDto paginationDto, string sp)
        {
            try
            {
                Collection<DbParameters> parameters = new();
                parameters.Add(new DbParameters { Name = "@Search_Query", Value = paginationDto.searchQuery ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Sort_Column_Name", Value = paginationDto.OrderBy ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Start_index", Value = paginationDto.StartIndex, DBType = DbType.Int64 });
                parameters.Add(new DbParameters { Name = "@Page_Size", Value = paginationDto.PageSize, DBType = DbType.Int64 });
                if(paginationDto.FromDate != null && paginationDto.ToDate != null)
                {
                    parameters.Add(new DbParameters { Name = "@FromDate", Value = paginationDto.FromDate, DBType = DbType.Date });
                    parameters.Add(new DbParameters { Name = "@ToDate", Value = paginationDto.ToDate , DBType = DbType.Date });

                }
                //IList<Book> books = DbClient.ExecuteProcedure<Book>("[dbo].[Get_Books_List]", parameters);
                IList<T> data = DbClient.ExecuteProcedure<T>(sp, parameters);

                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IList<Student> GetFromToDateStudents<Student>(PaginationDto paginationDto, string sp)
        {
            try
            {
                Collection<DbParameters> parameters = new();
                if (paginationDto.FromDate != null && paginationDto.ToDate != null)
                {
                    parameters.Add(new DbParameters { Name = "@FromDate", Value = paginationDto.FromDate, DBType = DbType.Date });
                    parameters.Add(new DbParameters { Name = "@ToDate", Value = paginationDto.ToDate, DBType = DbType.Date });

                }
                //IList<Book> books = DbClient.ExecuteProcedure<Book>("[dbo].[Get_Books_List]", parameters);
                IList<Student> data = DbClient.ExecuteProcedure<Student>(sp, parameters);

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
                table.Columns.Add("IsConfirmed");
                table.Columns.Add("IsRejected");

                var row = table.NewRow();
                row["FirstName"] = studentUpdateDto.FirstName;
                row["LastName"] = studentUpdateDto.LastName;
                row["BirthDate"] = ((DateTime)studentUpdateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentUpdateDto.CourseId;
                row["UserName"] = studentUpdateDto.UserName;
                row["Password"] = studentUpdateDto.Password;
                row["Email"] = studentUpdateDto.Email;
                row["IsConfirmed"] = studentUpdateDto.IsConfirmed;
                row["IsRejected"] = studentUpdateDto.IsRejected;
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
                table.Columns.Add("IsConfirmed");
                table.Columns.Add("IsRejected");

                var row = table.NewRow();
                row["FirstName"] = studentCreateDto.FirstName;
                row["LastName"] = studentCreateDto.LastName;
                row["BirthDate"] = ((DateTime)studentCreateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentCreateDto.CourseId;
                row["UserName"] = studentCreateDto.UserName;
                row["Password"] = studentCreateDto.Password;
                row["Email"] = studentCreateDto.Email;
                row["IsConfirmed"] = studentCreateDto.IsConfirmed;
                row["IsRejected"] = studentCreateDto.IsRejected;

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
        public void AddEmailAttachments(Byte[] attachment, string fileName, int scheduledEmailId, string query)
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
                    jwtClaims.RoleId = 3;
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
            else if ((controllerName == "Student" && methodName == "UpdateStudent") || (controllerName == "Student" && methodName == "ApproveRejectStudentRequest"))
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
            else if ((controllerName == "Login" && methodName == "ProfessorDetails") || (controllerName == "Email" && methodName == "GetScheduledEmailById"))
            {
                return Convert.ToInt32(dataObj);
            }
            else if ((controllerName == "Student" && methodName == "DeleteStudent") || (controllerName == "ProfessorHod" && methodName == "GetRecordsCount"))
            {
                return Convert.ToInt32(dataObj);
            }
            else if (controllerName == "Course" && methodName == "GetCourse")
            {
                return Convert.ToInt32(dataObj);
            }
            else if ((controllerName == "Student" && methodName == "GetAllStudents")
                || (controllerName == "Book" && methodName == "GetAllBooks")
                || (controllerName == "Email" && methodName == "GetScheduledEmails")
                || (controllerName == "Student" && methodName == "GetAllPendingStudents")
                || (controllerName == "ProfessorHod" && methodName == "GetAllProfessors")
                || (controllerName == "ProfessorHod" && methodName == "GetAllBlockedProfessors")
                || (controllerName == "ProfessorHod" && methodName == "GetAllQueries")
                || (controllerName == "Student" && methodName == "ExportStudentList"))
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
            else if ((controllerName == "Book" && methodName == "GetBook") 
                    || (controllerName == "Currency" && methodName == "GetRateAlerts")
                    || (controllerName == "Currency" && methodName == "GetRateAlertById")
                    || (controllerName == "Currency" && methodName == "RemoveRateAlert")
                    || (controllerName == "ProfessorHod") && methodName == "GetQueryDetail")
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
            else if ((controllerName == "Student" && methodName == "GetStudentByEmail") || (controllerName == "Student" && methodName == "GetStudentByUserName"))
            {
                return Convert.ToString(dataObj);
            }
            else if ((controllerName == "ProfessorHod" && methodName == "BlockUnblockProfessor"))
            {
                return GetDataModel<ProfessorHod>(dataObj);
            }
            else if (controllerName == "ProfessorHod" && methodName == "BlockUnblockStudent")
            {
                return GetDataModel<Student>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "GetExchangeRates")
            {
                return GetDataModel<ExchangeRate>(dataObj);
            }
            else if ((controllerName == "Currency" && methodName == "UpsertRateAlert") || (controllerName == "Currency" && methodName == "GetCurrencyPairRate"))
            {
                return GetDataModel<CurrencyPairDto>(dataObj);
            }
            else if((controllerName == "ProfessorHod" && methodName == "AddQueries") || (controllerName == "ProfessorHod" && methodName == "SendReplyEmail"))
            {
                return GetDataModel<QueriesDto>(dataObj);
            }
            else if(controllerName == "Student" && methodName == "GetStudentsCountFromDates")
            {
                return GetDataModel<StudentListCountFromDateDto>(dataObj);
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

        public ProfessorHod ProfessorBlockUnblockDetails(int userId)
        {
            string query = "SELECT IsBlocked FROM ProfessorHod Where Id = " + userId;
            return DbClient.ExecuteOneRecordProcedureWithQuery<ProfessorHod>(query, null);
        }

        public Student StudentBlockUnblockDetails(int userId)
        {
            string query = "SELECT IsBlocked FROM Students Where StudentId = " + userId;
            return DbClient.ExecuteOneRecordProcedureWithQuery<Student>(query, null);
        }

        public bool AprroveRejectRequest(StudentUpdateDto studentUpdateDto)
        {
            string Sql = "[dbo].[Approve_Reject_Student_Request]";
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@StudentId", Value = studentUpdateDto.StudentId, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@ApproveReject", Value = studentUpdateDto.ApproveReject, DBType = DbType.Boolean });
            DbClient.ExecuteProcedure(Sql, parameters, ExecuteType.ExecuteNonQuery);
            return true;
        }

        public ExchangeRate getExchangeRate(ExchangeRate exchangeRate)
        {
            try
            {
                string startDate = exchangeRate.StartDate.ToString("yyyy-MM-dd");
                string endDate = exchangeRate.EndDate.ToString("yyyy-MM-dd");
                string url = $"https://api.apilayer.com/exchangerates_data/timeseries?start_date={startDate}&end_date={endDate}"
                    + $"&base={exchangeRate.BaseCurrency}&symbols={exchangeRate.ToCurrency}";

                _httpClient.DefaultRequestHeaders.Add("apikey", "AhKWUYzt9yX56mBnd6ZAQKFts0jgUzrS");
                HttpResponseMessage response = _httpClient.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var exchangeRatesResponse = JsonConvert.DeserializeObject<ExchangeRate>(responseBody);
                return exchangeRatesResponse;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }

        }

        public void AddExchangeRates(ExchangeRate exchangeRate)
        {
            try
            {
                var table = new DataTable();
                table.Columns.Add("StartDate");
                table.Columns.Add("EndDate");
                table.Columns.Add("BaseCurrency");
                table.Columns.Add("ToCurrency");
                table.Columns.Add("Rate");

                exchangeRate.ratesWithDate = new();
                foreach (var entry in exchangeRate.Rates)
                {
                    var date = entry.Key;
                    var dailyRates = entry.Value;
                    if (dailyRates.TryGetValue(exchangeRate.ToCurrency, out var rate))
                    {
                        exchangeRate.ratesWithDate.Add(date, rate);
                    }
                }
                string ratesWithDateJson = JsonConvert.SerializeObject(exchangeRate.ratesWithDate);

                var row = table.NewRow();
                row["StartDate"] = exchangeRate.StartDate.ToString("yyyy-MM-dd");
                row["EndDate"] = exchangeRate.EndDate.ToString("yyyy-MM-dd");
                row["BaseCurrency"] = exchangeRate.BaseCurrency;
                row["ToCurrency"] = exchangeRate.ToCurrency;
                row["Rate"] = ratesWithDateJson;

                table.Rows.Add(row);
                string query = "[dbo].[Add_ExchangeRates]";
                Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@exchangeRate", Value = table, DBType = DbType.Object, TypeName = "[ExchangeRate]" }
                };
                DbClient.ExecuteProcedure(query, parameters, ExecuteType.ExecuteNonQuery);
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public ExchangeRate GetExchangeRateDetails(ExchangeRate exchangeRate)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@startDate", Value = exchangeRate.StartDate, DBType = DbType.Date },
                    new DbParameters() { Name = "@endDate", Value = exchangeRate.EndDate, DBType = DbType.Date },
                    new DbParameters() { Name = "@baseCurrency", Value = exchangeRate.BaseCurrency, DBType = DbType.String},
                    new DbParameters() { Name = "@toCurrency", Value = exchangeRate.ToCurrency, DBType = DbType.String }

                };
            ExchangeRate newExchangeRate = DbClient.ExecuteOneRecordProcedure<ExchangeRate>("[dbo].[Get_ExchangeRates]", parameters);
            return newExchangeRate;
        }

        public CurrencyPairDto GetCurrencyPairData(string currencyPair)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@currencyPair", Value = currencyPair, DBType = DbType.String},
                };
            CurrencyPairDto currencyPairDto = DbClient.ExecuteOneRecordProcedure<CurrencyPairDto>("[dbo].[Get_Currency_pair]", parameters);
            return currencyPairDto;
        }

        public void UpsertRateAlert(CurrencyPairDto currencyPairDto)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@rateAlertId", Value = currencyPairDto.RateAlertId , DBType = DbType.Int32},
                    new DbParameters() { Name = "@rate", Value = currencyPairDto.Rate , DBType = DbType.Decimal},
                    new DbParameters() { Name = "@expectedRate", Value = currencyPairDto.AskRate , DBType = DbType.Decimal},
                    new DbParameters() { Name = "@email", Value = currencyPairDto.Email, DBType = DbType.String},
                    new DbParameters() { Name = "@currencyPair", Value = currencyPairDto.CurrencyPair , DBType = DbType.String},
                    new DbParameters() { Name = "@studentId", Value = currencyPairDto.StudentId , DBType = DbType.Int32},
                };
            DbClient.ExecuteProcedure("[dbo].[Upsert_RateAlert]", parameters, ExecuteType.ExecuteNonQuery);
        }

        public IList<CurrencyPairDto> GetRateAlerts(int StudentId)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@studentId", Value = StudentId, DBType = DbType.Int32},
                };
            IList<CurrencyPairDto> currencyPairDto = DbClient.ExecuteProcedure<CurrencyPairDto>("[dbo].[Get_Rate_Alerts]", parameters);
            return currencyPairDto;
        }

        public CurrencyPairDto GetRateAlertById(int RateAlertId)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@rateAlertId", Value = RateAlertId, DBType = DbType.Int32},
                };
            CurrencyPairDto currencyPairDto = DbClient.ExecuteOneRecordProcedure<CurrencyPairDto>("[dbo].[Get_RateAlert_ById]", parameters);
            return currencyPairDto;
        }

        public void RemoveRateAlert(int RateAlertId)
        {
            string query = "UPDATE RateAlerts SET IsCompleted = 1 where RateAlertId =" + RateAlertId;
            DbClient.ExecuteProcedureWithQuery(query, null,ExecuteType.ExecuteNonQuery);
        }

        public void AddQueries(QueriesDto queriesDto)
        {
            var table = new DataTable();
            table.Columns.Add("TicketNumber");
            table.Columns.Add("StudentId");
            table.Columns.Add("Subject");
            table.Columns.Add("Body");

            var row = table.NewRow();
            row["TicketNumber"] = queriesDto.TicketNumber;
            row["StudentId"] = queriesDto.StudentId;
            row["Subject"] = queriesDto.Subject;
            row["Body"] = queriesDto.Body;

            table.Rows.Add(row);
            string query = "[dbo].[Add_Query]";
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@query", Value = table , DBType = DbType.Object, TypeName = "[dbo].[Query_table_type]"},
                };
            DbClient.ExecuteProcedure(query, parameters, ExecuteType.ExecuteNonQuery);
        }

        public QueriesDto GetQueryDetails(int QueryId)
        {
            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@queryId", Value = QueryId, DBType = DbType.Int32},
                };
            QueriesDto queriesDto = DbClient.ExecuteOneRecordProcedure<QueriesDto>("[dbo].[Query_Detail_ById]", parameters);
            return queriesDto;
        }

        public RecordsCountDto GetRecordsCounts(int id)
        {

            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@id", Value = id, DBType = DbType.Int32},
                };
            RecordsCountDto recordsCountDto = DbClient.ExecuteOneRecordProcedure<RecordsCountDto>("[dbo].[Get_Todays_Records_Count]", parameters);
            return recordsCountDto;
        }

        public IList<StudentListCountFromDateDto> GetStudentsCountFromDates(StudentListCountFromDateDto studentListCountFromDateDto)
        {

            Collection<DbParameters> parameters = new()
                {
                    new DbParameters() { Name = "@FromDate", Value = studentListCountFromDateDto.FromDate, DBType = DbType.Date},
                    new DbParameters() { Name = "@ToDate", Value = studentListCountFromDateDto.ToDate, DBType = DbType.Date},
                };
            IList<StudentListCountFromDateDto> studentsCount = DbClient.ExecuteProcedure<StudentListCountFromDateDto>("[dbo].[Get_StudentList_Count]", parameters);
            return studentsCount;
        }
    }
}
