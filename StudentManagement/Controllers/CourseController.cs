using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;

namespace StudentManagement_API.Controllers
{
    [Route("StudentApi/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private APIResponse _response;
        private readonly IJwtServices _jwtService;
        public IStudentServices _studentServices;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;



        public CourseController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices,
            IConfiguration configuration, IMapper mapper)
        {
            this._response = new();
            _studentServices = studentServices;
            this._jwtService = jwtService;
            _professorHodServices = professorHodServices;
            _mapper = mapper;
            _configuration = configuration;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet()]
        public ActionResult<APIResponse> GetAllCourses()
        {
            IList<Course> courses = _studentServices.GetRecordsWithoutPagination<Course>("[dbo].[Get_All_Courses]","Courses");
            if (courses.Count > 0)
            {
                RoleBaseResponse<IList<Course>> roleBase = new()
                {
                    data = courses,
                };
                _response.result = roleBase;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            else
            {
                _response.ErroMessages = new List<string> { "Courses Not Found" };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NotFound;
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{courseId:int}", Name = "GetCourse")]
        public ActionResult<APIResponse> GetCourse(int courseId)
        {
            try
            {
                if (courseId == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErroMessages = new List<string> { "Invalid courseId" };
                    _response.IsSuccess = false;
                    return _response;
                }
                Course course = _studentServices.GetData<Course>("Select * From Courses where CourseId = " + courseId, "Course" + courseId);
                if (course.CourseId > 0)
                {
                    _response.result = course;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.ErroMessages = new List<string> { "Course Not Found" };
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
        public ActionResult<APIResponse> CreateCourse([FromBody] CourseCreateDto courseCreateDto)
        {
            try
            {
                string sql = "Insert into Courses(CourseName) values (@CourseName)";

                _studentServices.InsertCourse(courseCreateDto, sql);
                RoleBaseResponse<bool> roleBaseResponse = new()
                {
                    data = true,
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
