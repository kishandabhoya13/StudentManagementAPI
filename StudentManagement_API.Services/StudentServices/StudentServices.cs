using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using static DemoApiWithoutEF.Utilities.Enums;

namespace StudentManagement_API.Services
{
    public class StudentServices : IStudentServices
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IJwtServices _jwtService;
        private readonly IMapper _mapper;
        public StudentServices(IConfiguration configuration, IJwtServices jwtService, IMapper mapper)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public T GetData<T>(string query)
        {
            T obj = DbClient.ExecuteOneRecordProcedureWithQuery<T>(query, null);
            return obj;
        }

        public T GetStudent<T>(string Procedure, int Id)
        {
            Collection<DbParameters> parameters = new();
            parameters.Add(new DbParameters { Name = "@StudentId", Value = Id, DBType = DbType.Int64 });
            T obj = DbClient.ExecuteOneRecordProcedure<T>(Procedure, parameters);
            return obj;
        }

        public IList<Student> GetDataWithPegination(PaginationDto paginationDto)
        {
            try
            {
                Collection<DbParameters> parameters = new();
                parameters.Add(new DbParameters { Name = "@Search_Query", Value = paginationDto.searchQuery ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Sort_Column_Name", Value = paginationDto.OrderBy ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Start_index", Value = paginationDto.StartIndex, DBType = DbType.Int64 });
                parameters.Add(new DbParameters { Name = "@Page_Size", Value = paginationDto.PageSize, DBType = DbType.Int64 });

                return DbClient.ExecuteProcedure<Student>("[dbo].[Get_Students_List]", parameters);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IList<T> GetRecordsWithoutPagination<T>(string ProcedureName)
        {
            IList<T> list = DbClient.ExecuteProcedure<T>(ProcedureName, null);
            return list;
        }

        public IList<Book> GetBooksWithPegination(PaginationDto paginationDto)
        {
            try
            {
                Collection<DbParameters> parameters = new();
                parameters.Add(new DbParameters { Name = "@Search_Query", Value = paginationDto.searchQuery ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Sort_Column_Name", Value = paginationDto.OrderBy ?? "", DBType = DbType.String });
                parameters.Add(new DbParameters { Name = "@Start_index", Value = paginationDto.StartIndex, DBType = DbType.Int64 });
                parameters.Add(new DbParameters { Name = "@Page_Size", Value = paginationDto.PageSize, DBType = DbType.Int64 });

                return DbClient.ExecuteProcedure<Book>("[dbo].[Get_Books_List]", parameters);
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

                var row = table.NewRow();
                row["FirstName"] = studentUpdateDto.FirstName;
                row["LastName"] = studentUpdateDto.LastName;
                row["BirthDate"] = ((DateTime)studentUpdateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentUpdateDto.CourseId;
                row["UserName"] = studentUpdateDto.UserName;
                row["Password"] = studentUpdateDto.Password;
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
                var row = table.NewRow();
                row["FirstName"] = studentCreateDto.FirstName;
                row["LastName"] = studentCreateDto.LastName;
                row["BirthDate"] = ((DateTime)studentCreateDto.BirthDate).ToString("MM-dd-yyyy");
                row["CourseId"] = studentCreateDto.CourseId;
                row["UserName"] = studentCreateDto.UserName;
                row["Password"] = studentCreateDto.Password;
                table.Rows.Add(row);

                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@Student_Details", Value = table, DBType = DbType.Object, TypeName = "Student_Details" });
                DbClient.ExecuteProcedure("Add_Student_Details", parameters, ExecuteType.ExecuteNonQuery);

            }
        }

        public void DeleteStudent(int StudentId)
        {
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@Id", Value = StudentId, DBType = DbType.Object, TypeName = "Student_Details" });
            DbClient.ExecuteProcedureWithQuery("Delete From Students where StudentId=@Id", parameters, ExecuteType.ExecuteNonQuery);
        }

        public Student GetLoginStudentDetails(StudentLoginDto studentLoginDto)
        {
            try
            {
                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@UserName", Value = studentLoginDto.UserName, DBType = DbType.String });
                parameters.Add(new DbParameters() { Name = "@PassWord", Value = studentLoginDto.Password, DBType = DbType.String });
                Student student = DbClient.ExecuteOneRecordProcedure<Student>("[dbo].[Get_UserName_Password]", parameters);
                JwtClaims jwtClaims = _mapper.Map<JwtClaims>(student);
                jwtClaims.Id = student.StudentId;
                student.JwtToken = _jwtService.GenerateToken(jwtClaims);
                if (student.StudentId != 0)
                {
                    UpdateJwtToken(student.JwtToken, student.StudentId);
                }
                return student;
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
            else if (controllerName == "Student" && methodName == "DeleteStudent")
            {
                return Convert.ToInt32(dataObj);
            }
            else if (controllerName == "Course" && methodName == "GetCourse")
            {
                return Convert.ToInt32(dataObj);
            }
            else if ((controllerName == "Student" && methodName == "GetAllStudents") || (controllerName == "Book" && methodName == "GetAllBooks"))
            {
                return GetDataModel<PaginationDto>(dataObj);
            }
            else if (controllerName == "Course" && methodName == "CreateCourse")
            {
                return GetDataModel<CourseCreateDto>(dataObj);
            }
            else if ((controllerName == "Book" && methodName == "CreateBook") || (controllerName == "Book" && methodName == "UpdateBook"))
            {
                return GetDataModel<Book>(dataObj);
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
            parameters.Add(new DbParameters() { Name = "@photoName", Value = book.PhotoName ?? "", DBType = DbType.String });

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
            }
            if(book.Photo != null)
            {
                parameters.Add(new DbParameters() { Name = "@photo", Value = book.PhotoName ?? "", DBType = DbType.String });
            }
            DbClient.ExecuteProcedure("[dbo].[add_edit_books]", parameters, ExecuteType.ExecuteNonQuery);
        }
    }
}
