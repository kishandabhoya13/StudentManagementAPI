using AutoMapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.Data;
using System.Runtime.CompilerServices;

namespace StudentManagement_API.Services
{
    public class StudentServices : IStudentServices
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        public StudentServices(IConfiguration configuration, IJwtService jwtService, IMapper mapper)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _jwtService = jwtService;
            _mapper = mapper;
        }

        public DataTable GetData(string query)
        {
            SqlConnection conn = new(connectionString);
            conn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(query, conn);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            DataTable dt = ds.Tables[0];
            return dt;
        }

        public List<Student> GetDataWithPegination(PaginationDto paginationDto)
        {
            try
            {
                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand("[dbo].[Get_Students_List]", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search_Query", paginationDto.searchQuery ?? "");
                cmd.Parameters.AddWithValue("@Sort_Column_Name", paginationDto.OrderBy ?? "");
                cmd.Parameters.AddWithValue("@Sort_Column_Direction", paginationDto.OrderDirection ?? "");
                cmd.Parameters.AddWithValue("@Start_index", paginationDto.StartIndex);
                cmd.Parameters.AddWithValue("@Page_Size", paginationDto.PageSize);
                con.Open();

                using var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                using var ds = new DataSet();
                da.Fill(ds);
                List<Student> students = new();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        Student student = new()
                        {
                            UserName = ds.Tables[0].Rows[i]["UserName"].ToString() ?? "",
                            Password = ds.Tables[0].Rows[i]["PassWord"].ToString() ?? "",
                            FirstName = ds.Tables[0].Rows[i]["FirstName"].ToString() ?? "",
                            LastName = ds.Tables[0].Rows[i]["LastName"].ToString() ?? "",
                            BirthDate = (DateTime)ds.Tables[0].Rows[i]["BirthDate"],
                            Dob = ((DateTime)ds.Tables[0].Rows[i]["BirthDate"]).ToString("dd-MMM-yyyy"),
                            CourseId = (int)ds.Tables[0].Rows[i]["CourseId"],
                            CourseName = ds.Tables[0].Rows[i]["CourseName"].ToString() ?? "",
                            StudentId = (int)ds.Tables[0].Rows[i]["StudentId"],
                        };
                        students.Add(student);
                    }
                }
                return students;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public int GetDataCount(string searchQuery)
        {
            try
            {
                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand("[dbo].[Get_Total_Record_Count]", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Search_Query", searchQuery ?? "");
                con.Open();
                int Count = 0;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Count = reader.GetInt32(0);
                    }
                }
                return Count;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
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

                        command.CommandType = CommandType.StoredProcedure;

                        var parameter = command.CreateParameter();
                        parameter.TypeName = "dbo.Student_Details";
                        parameter.Value = table;
                        parameter.ParameterName = "@Student_Details";
                        command.Parameters.Add(parameter);
                        command.Parameters.AddWithValue("@StudentId", studentUpdateDto.StudentId);
                        //command.Parameters.AddWithValue("@FirstName", studentUpdateDto.FirstName);
                        //command.Parameters.AddWithValue("@LastName", studentUpdateDto.LastName);
                        //command.Parameters.AddWithValue("@BirthDate", studentUpdateDto.BirthDate);
                        //command.Parameters.AddWithValue("@CourseId", studentUpdateDto.CourseId);
                        //command.Parameters.AddWithValue("@UserName", studentUpdateDto.UserName);
                        //command.Parameters.AddWithValue("@Password", studentUpdateDto.Password);

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
                        row["BirthDate"] = ((DateTime)studentUpdateDto.BirthDate).ToString("MM-dd-yyyy");
                        row["CourseId"] = studentCreateDto.CourseId;
                        row["UserName"] = studentCreateDto.UserName;
                        row["Password"] = studentCreateDto.Password;
                        table.Rows.Add(row);

                        command.CommandType = CommandType.StoredProcedure;

                        var parameter = command.CreateParameter();
                        parameter.TypeName = "dbo.Student_Details";
                        parameter.Value = table;
                        parameter.ParameterName = "@Student_Details";
                        command.Parameters.Add(parameter);


                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteStudent(int StudentId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "Delete From Students where StudentId=@Id";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", StudentId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Student GetLoginStudentDetails(StudentLoginDto studentLoginDto)
        {
            try
            {
                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand("[dbo].[Get_UserName_Password]", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", studentLoginDto.UserName);
                cmd.Parameters.AddWithValue("@PassWord", studentLoginDto.Password);

                con.Open();

                using var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                using var ds = new DataSet();
                da.Fill(ds);
                Student student = new();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        student.UserName = ds.Tables[0].Rows[i]["UserName"].ToString() ?? "";
                        student.Password = ds.Tables[0].Rows[i]["PassWord"].ToString() ?? "";
                        student.FirstName = ds.Tables[0].Rows[i]["FirstName"].ToString() ?? "";
                        student.LastName = ds.Tables[0].Rows[i]["LastName"].ToString() ?? "";
                        student.BirthDate = (DateTime)ds.Tables[0].Rows[i]["BirthDate"];
                        student.CourseId = (int)ds.Tables[0].Rows[i]["CourseId"];
                        student.StudentId = (int)ds.Tables[0].Rows[i]["StudentId"];
                    }
                }
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
            string query = "Update Students SET JwtToken = @JwtToken Where StudentId = @Id";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", StudentId);
                    command.Parameters.AddWithValue("@JwtToken", jwtToken);
                    command.ExecuteNonQuery();
                }
            }
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
            else if (controllerName == "Student" && methodName == "GetAllStudents")
            {
                return Convert.ToString(dataObj);
            }
            else if (controllerName == "Course" && methodName == "CreateCourse")
            {
                return GetDataModel<CourseCreateDto>(dataObj);
            }
            else
            {
                return null;
            }
        }

        public void InsertCourse(CourseCreateDto? courseCreateDto, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CourseName", courseCreateDto.Name);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
