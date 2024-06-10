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
        public StudentServices(IConfiguration configuration,IJwtService jwtService,IMapper mapper)
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
        public void UpsertStudent(StudentUpdateDto? studentUpdateDto, StudentCreateDto? studentCreateDto, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    if (studentUpdateDto != null)
                    {
                        command.Parameters.AddWithValue("@Id", studentUpdateDto.StudentId);
                        command.Parameters.AddWithValue("@FirstName", studentUpdateDto.FirstName);
                        command.Parameters.AddWithValue("@LastName", studentUpdateDto.LastName);
                        command.Parameters.AddWithValue("@BirthDate", studentUpdateDto.BirthDate);
                        command.Parameters.AddWithValue("@CourseId", studentUpdateDto.CourseId);
                        command.Parameters.AddWithValue("@UserName", studentUpdateDto.UserName);
                        command.Parameters.AddWithValue("@Password", studentUpdateDto.Password);

                    }
                    else
                    {
                        command.Parameters.AddWithValue("@FirstName", studentCreateDto.FirstName);
                        command.Parameters.AddWithValue("@LastName", studentCreateDto.LastName);
                        command.Parameters.AddWithValue("@BirthDate", studentCreateDto.BirthDate);
                        command.Parameters.AddWithValue("@CourseId", studentCreateDto.CourseId);
                        command.Parameters.AddWithValue("@UserName", studentCreateDto.UserName);
                        command.Parameters.AddWithValue("@Password", studentCreateDto.Password);


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
                if(student.StudentId != 0)
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

        public dynamic GetDynamicData(string controllerName, string methodName,object dataObj)
        {
            if(controllerName == "Student" && methodName == "GetStudent")
            {
                return Convert.ToInt32(dataObj);
            }
            else if((controllerName == "Student" && methodName == "LoginStudentDetails") || 
                    (controllerName == "ProfessorHod" && methodName == "LoginDetails"))
            {
                return GetDataModel<StudentLoginDto>(dataObj);
            }
            else if(controllerName == "Student" && methodName == "CreateStudent")
            {
                return GetDataModel<StudentCreateDto>(dataObj);
            }
            else if (controllerName == "Student" && methodName == "UpdateStudent")
            {
                return GetDataModel<StudentUpdateDto>(dataObj);
            }
            else if(controllerName == "Student" && methodName == "UpdateStudentJwtToken")
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
            else if(controllerName == "Student" && methodName == "GetAllStudents")
            {
                return Convert.ToString(dataObj);
            }
            else
            {
                return null;
            }
        }
    }
}
