using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;
using System.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StudentManagement_API.DataContext;
using static DemoApiWithoutEF.Utilities.Enums;
using System.Linq.Expressions;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorHodController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtServices _jwtService;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;


        public ProfessorHodController(IStudentServices studentServices, IJwtServices jwtService,
            IProfessorHodServices? professorHodServices, IConfiguration configuration, IMapper mapper)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices = professorHodServices;
            _configuration = configuration;
            _mapper = mapper;
        }

        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpGet("CheckLogin", Name = "LoginHodDetails")]
        //public ActionResult<APIResponse> LoginDetails([FromQuery] StudentLoginDto studentLoginDto)
        //{
        //    try
        //    {
        //        ProfessorHod professorHod = _professorHodServices.CheckUserNamePassword(studentLoginDto);
        //        _response.result = professorHod;
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

        [HttpPut("UpdateJwtToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse> UpdateProfessorHodJwtToken([FromBody] UpdateJwtDTo updateJwtDTo)
        {
            try
            {
                ProfessorHod professorHod = _studentServices.GetData<ProfessorHod>("Select Id from ProfessorHod where Id=" + updateJwtDTo.Id);
                if (professorHod.Id <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErroMessages = new List<string> { "ProfessorHod Not Found" };
                    return _response;
                }
                _professorHodServices.UpdateJwtToken(updateJwtDTo.token, updateJwtDTo.Id);
                //_studentServices.UpdateStudent(studentUpdateDto);
                _response.IsSuccess = true;
                _response.result = new RoleBaseResponse<bool>() { data = true };
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

        [Route("/SendEmail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        public ActionResult<APIResponse> SendEmail([FromBody] EmailLogs emailLogs)
        {
            try
            {
                if (emailLogs.SentDate.Value.Date != DateTime.Now.Date)
                {
                    string Scheduledsql = "[dbo].[Add_ScheduledEmail_Details]";
                    emailLogs.StudentId = emailLogs.StudentId == 0 ? null : emailLogs.StudentId;
                    int scheduledEmailId = _studentServices.AddEditScheduledEmailLogs(emailLogs, Scheduledsql);
                    //if (emailLogs.AttachmentsByte != null && emailLogs.AttachmentsByte.Count > 0)
                    //{
                    //    foreach(var attachment in emailLogs.AttachmentsByte)
                    //    {
                    //        _studentServices.AddEmailAttachments(attachment, scheduledEmailId, "[dbo].[Add_DataIn_EmailAttachments]");
                    //    }
                    //}

                    if (emailLogs.FileNameWithAttachments != null && emailLogs.FileNameWithAttachments.Count > 0)
                    {
                        foreach (var attachment in emailLogs.FileNameWithAttachments)
                        {
                            _studentServices.AddEmailAttachments(attachment.Value, attachment.Key, scheduledEmailId, "[dbo].[Add_DataIn_EmailAttachments]");
                        }
                    }

                }
                else
                {
                    foreach (var email in emailLogs.Emails)
                    {
                        MailMessage message = new MailMessage(_configuration["EmailCredential:From"], email);
                        SmtpClient client = new SmtpClient(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]));
                        System.Net.NetworkCredential basicCredential1 = new
                        System.Net.NetworkCredential(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = basicCredential1;
                        string mailbody = emailLogs.Body;
                        message.Subject = emailLogs.Subject;
                        message.Body = mailbody;
                        message.BodyEncoding = Encoding.UTF8;
                        message.IsBodyHtml = true;
                        message.Headers.Add("X-Message-ID", "123456789");
                        message.ReplyToList.Add(new MailAddress(_configuration["EmailCredential:From"]));
                        if (emailLogs.FileNameWithAttachments != null && emailLogs.FileNameWithAttachments.Count > 0)
                        {
                            foreach (var attachmentByte in emailLogs.FileNameWithAttachments)
                            {
                                string contentType;
                                string fileName = attachmentByte.Key; // Default to JPEG extension

                                // Check if the byte array represents a PDF
                                if (_studentServices.IsPDF(attachmentByte.Value))
                                {
                                    contentType = "application/pdf";
                                }
                                else
                                {
                                    contentType = "image/jpeg";
                                }
                                message.Attachments.Add(new Attachment(new MemoryStream(attachmentByte.Value), fileName, contentType));
                            }
                        }

                        try
                        {
                            string id = "123456789"; //Save the id in your database 
                            message.Headers.Add("Message-Id", String.Format("<{0}@{1}>", id.ToString(), "mail.example.com"));
                            client.Send(message);
                            string messageId = message.Headers["Message-ID"] ?? "";
                        }
                        catch (Exception)
                        {
                            string EmailLogSql = "[dbo].[Add_EmailLog_Details]";
                            emailLogs.Email = email;
                            emailLogs.IsSent = false;
                            _studentServices.AddEmailLogs(emailLogs, EmailLogSql);
                        }
                        string sql = "[dbo].[Add_EmailLog_Details]";
                        emailLogs.Email = email;
                        emailLogs.IsSent = true;
                        _studentServices.AddEmailLogs(emailLogs, sql);
                    }
                }

                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
            }

            catch (Exception ex)
            {

                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAllProfessors")]
        public ActionResult<APIResponse> GetAllProfessors(PaginationDto paginationDto)
        {
            if (paginationDto.StartIndex < 0 || paginationDto.PageSize < 0)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            var role = "";
            if (_jwtService.ValidateToken(paginationDto.JwtToken, out JwtSecurityToken jwtSecurityToken))
            {
                role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            }

            IList<ProfessorHod> professorHods = _studentServices.GetDataWithPagination<ProfessorHod>(paginationDto, "[dbo].[Get_Professors_list]");
            int totalItems = professorHods.Count > 0 ? professorHods.FirstOrDefault(x => x.Id != 0)?.TotalRecords ?? 0 : 0;
            int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
            RoleBaseResponse<IList<ProfessorHod>> roleBaseResponse = new()
            {
                data = professorHods,
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut]
        public ActionResult<APIResponse> BlockUnblockProfessor(ProfessorHod professorHod)
        {
            try
            {
                string sql = "";
                if (professorHod.IsBlocked)
                {
                    sql = "Update ProfessorHod Set IsBlocked = 1 Where Id = " + professorHod.Id;

                }
                else
                {
                    sql  = "Update ProfessorHod Set IsBlocked = 0 Where Id = " + professorHod.Id;
                }
                DbClient.ExecuteProcedureWithQuery(sql, null, ExecuteType.ExecuteNonQuery);
                _response.result = new RoleBaseResponse<bool>() { data = true }; ;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.result = new RoleBaseResponse<bool>() { data = false }; ;
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;

        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("BlockUnblockStudent")]
        public ActionResult<APIResponse> BlockUnblockStudent(Student student)
        {
            try
            {
                string sql = "";
                if (student.IsBlocked)
                {
                    sql = "Update Students Set IsBlocked = 0 Where StudentId = " + student.StudentId;

                }
                else
                {
                    sql = "Update Students Set IsBlocked = 1 Where StudentId = " + student.StudentId;
                }
                DbClient.ExecuteProcedureWithQuery(sql, null, ExecuteType.ExecuteNonQuery);
                _response.result = new RoleBaseResponse<bool>() { data = true }; ;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.result = new RoleBaseResponse<bool>() { data = false }; ;
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;

        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAllBlockedProfessors")]
        public ActionResult<APIResponse> GetAllBlockedProfessors(PaginationDto paginationDto)
        {
            if (paginationDto.StartIndex < 0 || paginationDto.PageSize < 0)
            {
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Unauthorized;
                return _response;
            }
            var role = "";
            if (_jwtService.ValidateToken(paginationDto.JwtToken, out JwtSecurityToken jwtSecurityToken))
            {
                role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            }

            IList<ProfessorHod> professorHods = _studentServices.GetDataWithPagination<ProfessorHod>(paginationDto, "[dbo].[Get_blocked_Professors_list]");
            int totalItems = professorHods.Count > 0 ? professorHods.FirstOrDefault(x => x.Id != 0)?.TotalRecords ?? 0 : 0;
            int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
            RoleBaseResponse<IList<ProfessorHod>> roleBaseResponse = new()
            {
                data = professorHods,
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

    }
}
