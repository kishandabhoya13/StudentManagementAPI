using AutoMapper;
using Microsoft.Data.SqlClient;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.Data;

namespace StudentManagement_API.Services
{
    public class ProfessorHodServices : IProfessorHodServices
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        public ProfessorHodServices(IConfiguration configuration, IJwtService jwtService,IMapper mapper)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _jwtService = jwtService;
            _mapper = mapper;
        }


        public ProfessorHod CheckUserNamePassword(StudentLoginDto studentLoginDto)
        {
            try
            {
                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand("[dbo].[Get_ProfessorHod_UserName_Password]", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", studentLoginDto.UserName);
                cmd.Parameters.AddWithValue("@PassWord", studentLoginDto.Password);

                con.Open();

                using var da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                using var ds = new DataSet();
                da.Fill(ds);
                ProfessorHod professorHod= new();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        professorHod.UserName = ds.Tables[0].Rows[i]["UserName"].ToString() ?? "";
                        professorHod.Password = ds.Tables[0].Rows[i]["Password"].ToString() ?? "";
                        professorHod.FirstName = ds.Tables[0].Rows[i]["FirstName"].ToString() ?? "";
                        professorHod.LastName = ds.Tables[0].Rows[i]["LastName"].ToString() ?? "";
                        professorHod.BirthDate = (DateTime)ds.Tables[0].Rows[i]["BirthDate"];
                        professorHod.Id = (int)ds.Tables[0].Rows[i]["Id"];
                        professorHod.RoleId = (int)ds.Tables[0].Rows[i]["Type"];
                    }
                }
                JwtClaims jwtClaims = _mapper.Map<JwtClaims>(professorHod);
                professorHod.JwtToken = _jwtService.GenerateToken(jwtClaims);
                if (professorHod.Id!= 0)
                {
                    UpdateJwtToken(professorHod.JwtToken, professorHod.Id);
                }
                return professorHod;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UpdateJwtToken(string jwtToken, int Id)
        {
            string query = "Update [dbo].[ProfessorHod] SET JwtToken = @JwtToken Where Id = @Id";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", Id);
                    command.Parameters.AddWithValue("@JwtToken", jwtToken);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
