using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement_API.Models;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
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
        private readonly IJwtService _jwtService;
        public IStudentServices _studentServices;
        public CourseController(IStudentServices studentServices,IJwtService jwtService)
        {
            this._response = new();
            _studentServices = studentServices;
            this._jwtService = jwtService;
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
                DataTable dt = _studentServices.GetData("Select * From Courses where CourseId = " + courseId);
                Course course = new();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        course.Name = dr["CourseName"].ToString() ?? "";
                        course.Id = (int)dr["CourseId"];
                    }
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
    }
}
