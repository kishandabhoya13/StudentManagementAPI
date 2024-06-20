using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Collections.ObjectModel;
using System.Data;
using static DemoApiWithoutEF.Utilities.Enums;

namespace StudentManagement_API.Services
{
    public class ProfessorHodServices : IProfessorHodServices
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IJwtServices _jwtService;
        private readonly IMapper _mapper;
        public ProfessorHodServices(IConfiguration configuration, IJwtServices jwtService,IMapper mapper)
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
                Collection<DbParameters> parameters = new Collection<DbParameters>();
                parameters.Add(new DbParameters() { Name = "@UserName", Value = studentLoginDto.UserName, DBType = DbType.String });
                parameters.Add(new DbParameters() { Name = "@PassWord", Value = studentLoginDto.Password, DBType = DbType.String });
                ProfessorHod professorHod= DbClient.ExecuteOneRecordProcedure<ProfessorHod>("[dbo].[Get_ProfessorHod_UserName_Password]", parameters);

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

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@Id", Value = Id, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@JwtToken", Value = jwtToken, DBType = DbType.String });
            DbClient.ExecuteProcedureWithQuery("Update [dbo].[ProfessorHod] SET JwtToken = @JwtToken Where Id = @Id", parameters, ExecuteType.ExecuteNonQuery);
        }

        public bool IsAuthorized(ApiRequest apiRequest)
        {
            string query = "";
            int roleId = 0;
            if(apiRequest.MethodType == "IsInsert")
            {
                query = "SELECT RoleId FROM [dbo].[RoleAccess] WHERE RoleId = @RoleId AND PageName = @PageName AND IsInsert = 1";
            }
            else if(apiRequest.MethodType == "IsManaged")
            {
                query = "SELECT RoleId FROM [dbo].[RoleAccess] WHERE RoleId = @RoleId AND PageName = @PageName AND IsManaged = 1";
            }
            else
            {
                query = "SELECT RoleId FROM [dbo].[RoleAccess] WHERE RoleId = @RoleId AND PageName = @PageName AND IsViewed = 1";
            }
            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@RoleId", Value = apiRequest.RoleId, DBType = DbType.Int64 });
            parameters.Add(new DbParameters() { Name = "@PageName", Value = apiRequest.PageName, DBType = DbType.String });
            AllIdDto allIdDto = DbClient.ExecuteOneRecordProcedureWithQuery<AllIdDto>(query, parameters);
            roleId = allIdDto.RoleId;
            if(roleId != 0)
            {
                return true;
            }
            return false;
        }
    }
}
