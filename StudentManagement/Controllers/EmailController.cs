using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private APIResponse _response;
        private readonly IJwtServices _jwtService;
        public IStudentServices _studentServices;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;



        public EmailController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices, 
            IConfiguration configuration, IMapper mapper)
        {
            this._response = new();
            _studentServices = studentServices;
            this._jwtService = jwtService;
            _professorHodServices = professorHodServices;
            _configuration = configuration;
            _mapper = mapper;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult<APIResponse> GetScheduledEmails(PaginationDto paginationDto)
        {
            try
            {
                if (paginationDto.StartIndex < 0)
                {
                    throw new ArgumentException("Valid Index");
                }
                var role = "";
                if (_jwtService.ValidateToken(paginationDto.JwtToken, out JwtSecurityToken jwtSecurityToken))
                {
                    role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
                }

                string cacheKey = "EmailList" + paginationDto.PageSize + paginationDto.StartIndex + paginationDto.searchQuery;

                IList<EmailLogs> emailLogs = _studentServices.GetDataWithPagination<EmailLogs>(paginationDto,cacheKey ,"[dbo].[Get_ScheduledEmail_All_Details]");
                int totalItems = emailLogs.Count > 0 ? emailLogs.FirstOrDefault(x => x.ScheduledEmailId != 0)?.TotalRecords ?? 0 : 0;
                int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
                RoleBaseResponse<IList<EmailLogs>> roleBaseResponse = new()
                {
                    data = emailLogs,
                    Role = role,
                    StartIndex = paginationDto.StartIndex,
                    PageSize = paginationDto.PageSize,
                    TotalItems = totalItems,
                    TotalPages = TotalPages,
                    CurrentPage = (int)Math.Ceiling((double)paginationDto.StartIndex / paginationDto.PageSize)
                };
                _response.result = roleBaseResponse;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return _response;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErroMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.BadRequest;

                return _response;
            }

        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/GetScheduledEmailById")]
        public ActionResult<APIResponse> GetScheduledEmailById(int ScheduledEmailId)
        {
            try
            {
                if (ScheduledEmailId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErroMessages = new List<string> { "Invalid ScheduledEmailId" };
                    _response.IsSuccess = false;
                    return _response;
                }
                EmailLogs emailLogs = _studentServices.GetScheduledEmailById<EmailLogs>("[dbo].[get_scheduledEmail_by_Id]", ScheduledEmailId);
                if (emailLogs.ScheduledEmailId> 0)
                {
                    if(emailLogs.StudentId == null)
                    {
                        emailLogs.StudentId = 0;
                    }
                    IList<EmailLogs> attachments = _studentServices.GetAttachementsFromScheduledId(emailLogs.ScheduledEmailId); 
                    if(attachments.Count > 0)
                    {
                        emailLogs.AttachmentsByte = new();
                        foreach(var attachment in attachments)
                        {
                            emailLogs.AttachmentsByte.Add(attachment.AttachmentFile);
                        }
                    }
                    _response.result = emailLogs;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.ErroMessages = new List<string> { "ScheduledEmail was Not Fount" };
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



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> AddEditScheduledEmailLogs([FromBody] EmailLogs emailLogs)
        {
            try
            {
                //string sql = "INSERT INTO Students (FirstName,LastName,BirthDate,CourseId,UserName,PassWord)" +
                //     " VALUES (@FirstName, @LastName, @BirthDate, @CourseId,@UserName,@Password)";
                string sql = "[dbo].[Add_ScheduledEmail_Details]";
                _studentServices.AddEditScheduledEmailLogs(emailLogs, sql);
                _studentServices.UpdateAttachments(emailLogs, "[dbo].[Update_Attachments_ById]");
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

        [Route("/AddEmailLogs")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> AddEmailLogs([FromBody] EmailLogs emailLogs)
        {
            try
            {
                string sql = "[dbo].[Add_EmailLog_Details]";
                _studentServices.AddEmailLogs(emailLogs, sql);
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


        [HttpGet("DayWiseEmailCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> GetDayWiseEmailCount(EmailLogs emailLog)
        {
            try
            {

                IList<EmailLogs> emailLogs = _studentServices.GetDayWiseEmailCount(emailLog);
                RoleBaseResponse<IList<EmailLogs>> roleBaseResponse = new()
                {
                    data = emailLogs,
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
    }
}
