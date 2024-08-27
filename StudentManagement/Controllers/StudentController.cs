using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using StudentManagment_API.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Web.Helpers;

namespace StudentManagement_API.Controllers
{

    [Route("StudentApi/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtServices _jwtService;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        public StudentController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices,
            IConfiguration configuration, IMapper mapper)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices = professorHodServices;
            _configuration = configuration;
            _mapper = mapper;
        }

        [ServiceFilter(typeof(LogActionFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult<APIResponse> GetAllStudents(PaginationDto paginationDto)
        {
            var user = httpContextAccessor.HttpContext.User;
            if (paginationDto.StartIndex < 0 || paginationDto.PageSize < 0)
            {
                return _response;
            }
            var role = "";
            if (_jwtService.ValidateToken(paginationDto.JwtToken, out JwtSecurityToken jwtSecurityToken))
            {
                role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            }

            IList<Student> students = _studentServices.GetDataWithPagination<Student>(paginationDto, "[dbo].[Get_Students_List]");
            int totalItems = students.Count > 0 ? students.FirstOrDefault(x => x.StudentId != 0)?.TotalRecords ?? 0 : 0;
            int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
            RoleBaseResponse<IList<Student>> roleBaseResponse = new()
            {
                data = students,
                Role = role,
                StartIndex = paginationDto.StartIndex,
                PageSize = paginationDto.PageSize,
                TotalItems = totalItems,
                TotalPages = TotalPages,
                CurrentPage = (int)Math.Ceiling((double)paginationDto.StartIndex / paginationDto.PageSize),
                searchQuery = paginationDto.searchQuery,
            };

            _response.result = roleBaseResponse;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;



            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAllPendingStudents")]
        public ActionResult<APIResponse> GetAllPendingStudents(PaginationDto paginationDto)
        {
            if (paginationDto.StartIndex < 0 || paginationDto.PageSize < 0)
            {
                return _response;
            }
            var role = "";
            if (_jwtService.ValidateToken(paginationDto.JwtToken, out JwtSecurityToken jwtSecurityToken))
            {
                role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            }

            IList<Student> students = _studentServices.GetDataWithPagination<Student>(paginationDto, "[dbo].[Get_Pending_Students_List]");
            int totalItems = students.Count > 0 ? students.FirstOrDefault(x => x.StudentId != 0)?.TotalRecords ?? 0 : 0;
            int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
            RoleBaseResponse<IList<Student>> roleBaseResponse = new()
            {
                data = students,
                Role = role,
                StartIndex = paginationDto.StartIndex,
                PageSize = paginationDto.PageSize,
                TotalItems = totalItems,
                TotalPages = TotalPages,
                CurrentPage = (int)Math.Ceiling((double)paginationDto.StartIndex / paginationDto.PageSize),
                searchQuery = paginationDto.searchQuery,
            };

            _response.result = roleBaseResponse;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;



            return _response;
        }

        [ServiceFilter(typeof(LogActionFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        [Route("/GetEmailsAndStudentIds")]
        public ActionResult<APIResponse> GetEmailsAndStudentIds()
        {
            IList<EmailLogs> emailLogs = _studentServices.GetEmailsAndStudentIds();
            RoleBaseResponse<IList<EmailLogs>> roleBaseResponse = new()
            {
                data = emailLogs,
            };
            _response.result = roleBaseResponse;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return _response;

        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{studentId:int}", Name = "GetStudent")]
        [RoleBasedAuthorizeAttribute("1", "2")]
        public ActionResult<APIResponse> GetStudent(int studentId)
        {
            try
            {
                if (studentId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErroMessages = new List<string> { "Invalid StudentId" };
                    _response.IsSuccess = false;
                    return _response;
                }

                Student student = _studentServices.GetStudent<Student>("[dbo].[Get_Student_Details]", studentId);
                RoleBaseResponse<Student> roleBaseResponse = new()
                {
                    data = student
                };
                if (student.StudentId > 0)
                {
                    _response.result = roleBaseResponse;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.ErroMessages = new List<string> { "Student Not Fount" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

        }

        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpGet("CheckLogin", Name = "LoginStudentDetails")]
        //public ActionResult<APIResponse> LoginStudentDetails([FromQuery] StudentLoginDto studentLoginDto)
        //{
        //    try
        //    {
        //        Student student = _studentServices.GetLoginStudentDetails(studentLoginDto);
        //        _response.result = student;
        //        _response.StatusCode = HttpStatusCode.OK;
        //        _response.IsSuccess = true;
        //        return _response;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.ErroMessages = new List<string> { ex.ToString() };
        //        _response.IsSuccess = false;
        //        _response.StatusCode = HttpStatusCode.BadRequest;
        //        return _response;
        //    }
        //}

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> CreateStudent([FromBody] StudentCreateDto studentCreateDto)
        {
            try
            {
                //string sql = "INSERT INTO Students (FirstName,LastName,BirthDate,CourseId,UserName,PassWord)" +
                //     " VALUES (@FirstName, @LastName, @BirthDate, @CourseId,@UserName,@Password)";
                string sql = "Add_Student_Details";
                _studentServices.UpsertStudent(null, studentCreateDto, sql);
                _response.result = new RoleBaseResponse<bool>() { data = true };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpPut("UpdateJwtToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse> UpdateStudentJwtToken([FromBody] UpdateJwtDTo updateJwtDTo)
        {
            try
            {
                Student student = _studentServices.GetStudent<Student>("[dbo].[Get_Student_Details]", updateJwtDTo.Id);
                if (student.StudentId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErroMessages = new List<string> { "Student Not Found" };
                    return _response;
                }
                _studentServices.UpdateJwtToken(updateJwtDTo.token, updateJwtDTo.Id);
                //_studentServices.UpdateStudent(studentUpdateDto);
                _response.result = new RoleBaseResponse<bool>() { data = true };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse> UpdateStudent([FromBody] StudentUpdateDto studentUpdateDto)
        {
            try
            {
                Student student = _studentServices.GetStudent<Student>("[dbo].[Get_Student_Details]", studentUpdateDto.StudentId);
                if (student.StudentId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErroMessages = new List<string> { "Student Not Found" };
                    return _response;
                }
                string sql = "Update_Student_Details";
                //string sql = "Update Students SET FirstName = @FirstName, LastName = @LastName, BirthDate = @BirthDate, CourseId = @CourseId, UserName = @UserName, PassWord = @Password Where StudentId = @Id";
                _studentServices.UpsertStudent(studentUpdateDto, null, sql);
                //_studentServices.UpdateStudent(studentUpdateDto);
                _response.result = new RoleBaseResponse<bool>() { data = true };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpDelete("{StudentId:int}", Name = "DeleteStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> DeleteStudent(int StudentId)
        {
            try
            {
                _studentServices.DeleteStudent(StudentId);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/EmailFromStudentId")]
        public ActionResult<APIResponse> GetEmailFromStudentId([FromBody] EmailLogs emailLogs)
        {
            try
            {
                if (emailLogs.StudentId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErroMessages = new List<string> { "Invalid StudentId" };
                    _response.IsSuccess = false;
                    return _response;
                }
                EmailLogs emailLogs1 = _studentServices.GetStudent<EmailLogs>("[dbo].[Get_Email_from_StudentId]", emailLogs.StudentId ?? 0);
                if (emailLogs1.Email != null)
                {
                    RoleBaseResponse<EmailLogs> roleBaseResponse = new()
                    {
                        data = emailLogs1
                    };
                    _response.result = roleBaseResponse;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.ErroMessages = new List<string> { "Student Not Fount" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpGet("DayWiseCountStudentProf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> DayWiseCountStudentProf(CountStudentProfessorDto countStudentProfessorDto)
        {
            try
            {

                IList<CountStudentProfessorDto> list = _studentServices.GetDayWiseProfStudentCount(countStudentProfessorDto);
                RoleBaseResponse<IList<CountStudentProfessorDto>> roleBaseResponse = new()
                {
                    data = list,
                };
                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpPut("ApproveRejectStudentRequest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse> ApproveRejectStudentRequest(StudentUpdateDto studentUpdateDto)
        {
            try
            {

                _studentServices.AprroveRejectRequest(studentUpdateDto);
                MailMessage message = new MailMessage(_configuration["EmailCredential:From"], studentUpdateDto.Email);
                SmtpClient client = new SmtpClient(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]));
                System.Net.NetworkCredential basicCredential1 = new
                System.Net.NetworkCredential(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicCredential1;
                string mailbody = studentUpdateDto.Body ?? "";
                message.Subject = "Approve/Reject Sign Up Request";
                message.Body = mailbody;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Headers.Add("X-Message-ID", "123456789");
                message.ReplyToList.Add(new MailAddress(_configuration["EmailCredential:From"]));
                try
                {
                    string id = "123456789"; //Save the id in your database 
                    message.Headers.Add("Message-Id", String.Format("<{0}@{1}>", id.ToString(), "mail.example.com"));
                    client.Send(message);
                    string messageId = message.Headers["Message-ID"] ?? "";

                    EmailLogs emailLogs = new()
                    {
                        Email = studentUpdateDto.Email,
                        Body = studentUpdateDto.Body,
                        Subject = message.Subject,
                        SentBy = 1,
                        IsSent = true,

                    };
                    string EmailLogSql = "[dbo].[Add_EmailLog_Details]";
                    _studentServices.AddEmailLogs(emailLogs, EmailLogSql);
                }
                catch (Exception)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.Unauthorized;
                    return _response;
                }

                _response.result = new RoleBaseResponse<bool>() { data = true };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString()
    };
                _response.result = false;
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpGet("GetStudentByEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> GetStudentByEmail(string Email)
        {
            try
            {
                string sql = "SELECT Email FROM Students WHERE Email = '" + Email + "'";
                Student student = _studentServices.GetData<Student>(sql);
                RoleBaseResponse<Student> roleBaseResponse = new()
                {
                    data = student,
                };
                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.result = false;
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpGet("GetStudentByUserName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> GetStudentByUserName(string UserName)
        {
            try
            {
                string sql = "SELECT UserName FROM Students WHERE UserName = '" + UserName + "'";
                RoleBaseResponse<Student> roleBaseResponse = new();
                Student student = _studentServices.GetData<Student>(sql);
                if (student.UserName == null)
                {
                    string sql2 = "SELECT UserName FROM ProfessorHod WHERE UserName = '" + UserName + "'";
                    Student student2 = _studentServices.GetData<Student>(sql2);
                    roleBaseResponse.data = student2;
                }
                else
                {

                    roleBaseResponse.data = student;
                }
                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.result = false;
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }
        }

        [HttpGet("GetExchangeRates")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> GetExchangeRates(ExchangeRate exchangeRate)
        {
            ExchangeRate newExchangeRates = _studentServices.GetExchangeRateDetails(exchangeRate);
            if (newExchangeRates.Rate == null)
            {
                ExchangeRate exchangeRate1 = _studentServices.getExchangeRate(exchangeRate);
                exchangeRate1.StartDate = exchangeRate.StartDate;
                exchangeRate1.EndDate = exchangeRate.EndDate;
                exchangeRate1.BaseCurrency = exchangeRate.BaseCurrency;
                exchangeRate1.ToCurrency = exchangeRate.ToCurrency;
                _studentServices.AddExchangeRates(exchangeRate1);
                newExchangeRates = _studentServices.GetExchangeRateDetails(exchangeRate);
            }

            RoleBaseResponse<ExchangeRate> roleBaseResponse = new()
            {
                data = newExchangeRates,
            };
            _response.result = roleBaseResponse;
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("ExportStudentList")]
        public ActionResult<APIResponse> ExportStudentList(PaginationDto paginationDto)
        {
            try
            {
                IList<Student> students = _studentServices.GetFromToDateStudents<Student>(paginationDto, "[dbo].[fromDate_toDate_Students_List]");

                RoleBaseResponse<IList<Student>> roleBaseResponse = new()
                {
                    data = students,
                };

                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.result = false;
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetStudentsCountFromDates")]
        public ActionResult<APIResponse> GetStudentsCountFromDates(StudentListCountFromDateDto studentListCountFromDateDto)
        {

            try
            {
                IList<StudentListCountFromDateDto> studentCountList = _studentServices.GetStudentsCountFromDates(studentListCountFromDateDto);
                RoleBaseResponse<IList<StudentListCountFromDateDto>> roleBaseResponse = new()
                {
                    data = studentCountList,
                };
                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.result = false;
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
           
        }
    }
}
