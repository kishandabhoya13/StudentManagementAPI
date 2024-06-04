﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;

namespace StudentManagement_API.Controllers
{
    [Route("StudentApi/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtService _jwtService;
        public StudentController(IStudentServices studentServices, IJwtService jwtService)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult<APIResponse> GetAllStudents()
        {
            try
            {
                DataTable dt = _studentServices.GetData("Select * From Students");
                List<Student> students = new();
                foreach (DataRow dr in dt.Rows)
                {
                    Student student = new()
                    {
                        StudentId = (int)dr["StudentId"],
                        FirstName = dr["FirstName"].ToString() ?? "",
                        LastName = dr["LastName"].ToString() ?? "",
                        BirthDate = (DateTime)dr["BirthDate"],
                        CourseId = (int)dr["CourseId"],
                        UserName = dr["UserName"].ToString() ?? "",
                    };
                    students.Add(student);
                }

                _response.result = students;
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
                    return BadRequest(_response);
                }
                DataTable dt = _studentServices.GetData("Select * From Students where StudentId = " + studentId);
                Student student = new();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        student.StudentId = (int)dr["StudentId"];
                        student.FirstName = dr["FirstName"].ToString() ?? "";
                        student.LastName = dr["LastName"].ToString() ?? "";
                        student.BirthDate = (DateTime)dr["BirthDate"];
                        student.CourseId = (int)dr["CourseId"];
                        student.UserName = dr["UserName"].ToString() ?? "";
                    }
                    _response.result = student;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.ErroMessages = new List<string> { "Student Not Fount" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
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
                string sql = "INSERT INTO Students (FirstName,LastName,BirthDate,CourseId)" +
                     " VALUES (@FirstName, @LastName, @BirthDate, @CourseId)";

                _studentServices.UpsertStudent(null, studentCreateDto, sql);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
        }

        [HttpPut("UpdateJwtToken")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<APIResponse> UpdateStudent([FromBody] string token, int StudentId)
        {
            try
            {
                DataTable dt = _studentServices.GetData("Select FirstName from Students where StudentId=" + StudentId);
                if (dt.Rows.Count <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErroMessages = new List<string> { "Student Not Found" };
                    return NotFound(_response);
                }
                _studentServices.UpdateJwtToken(token, StudentId);
                //_studentServices.UpdateStudent(studentUpdateDto);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
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
                DataTable dt = _studentServices.GetData("Select FirstName from Students where StudentId=" + studentUpdateDto.StudentId);
                if (dt.Rows.Count <= 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.ErroMessages = new List<string> { "Student Not Found" };
                    return NotFound(_response);
                }
                string sql = "Update Students SET FirstName = @FirstName, LastName = @LastName, BirthDate = @BirthDate, CourseId = @CourseId Where StudentId = @Id";
                _studentServices.UpsertStudent(studentUpdateDto, null, sql);
                //_studentServices.UpdateStudent(studentUpdateDto);
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
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
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.ToString() };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

        }
    }
}