using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using StudentManagment_API.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;

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

        public StudentController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices = professorHodServices;
        }

        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[HttpGet]
        //public ActionResult<APIResponse> GetAllStudents(string jwtToken)
        //{
        //    try
        //    {
        //        var role = "";
        //        if (_jwtService.ValidateToken(jwtToken, out JwtSecurityToken jwtSecurityToken))
        //        {
        //            role = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
        //        }
        //        List<Student> students = _studentServices.GetAllData();
        //        RoleBaseResponse roleBaseResponse = new()
        //        {
        //            Students = students,
        //            Role = role
        //        };
        //        _response.result = roleBaseResponse;
        //        _response.IsSuccess = true;
        //        _response.StatusCode = HttpStatusCode.OK;
        //        return _response;
        //    }
        //    catch (Exception ex)
        //    {
        //        _response.IsSuccess = false;
        //        _response.ErroMessages = new List<string> { ex.Message };
        //        _response.StatusCode = HttpStatusCode.BadRequest;

        //        return _response;
        //    }

        //}

        [ServiceFilter(typeof(LogActionFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult<APIResponse> GetAllStudents(PaginationDto paginationDto)
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
            IList<Student> students = _studentServices.GetDataWithPegination(paginationDto);
            int totalItems = students.Count > 0 ? students.FirstOrDefault(x => x.StudentId != 0)?.TotalRecords ?? 0 : 0;
            int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
            RoleBaseResponse<Student> roleBaseResponse = new()
            {
                data = students,
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{studentId:int}", Name = "GetStudent")]
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
                if (student.StudentId > 0)
                {
                    _response.result = student;
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("CheckLogin", Name = "LoginStudentDetails")]
        public ActionResult<APIResponse> LoginStudentDetails([FromQuery] StudentLoginDto studentLoginDto)
        {
            try
            {
                Student student = _studentServices.GetLoginStudentDetails(studentLoginDto);
                _response.result = student;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
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
        public ActionResult<APIResponse> CreateStudent([FromBody] StudentCreateDto studentCreateDto)
        {
            try
            {
                //string sql = "INSERT INTO Students (FirstName,LastName,BirthDate,CourseId,UserName,PassWord)" +
                //     " VALUES (@FirstName, @LastName, @BirthDate, @CourseId,@UserName,@Password)";
                string sql = "Add_Student_Details";
                _studentServices.UpsertStudent(null, studentCreateDto, sql);
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
    }
}
