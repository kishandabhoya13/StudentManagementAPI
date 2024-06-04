using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Web.Http.Results;
using OkResult = System.Web.Http.Results.OkResult;

namespace StudentManagement_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MasterApiController : ControllerBase
    {
        private readonly IStudentServices _studentServices;
        private readonly IJwtService _jwtService;
        private APIResponse _response;
        public Dictionary<string, Type> controllers = new()
        {
            { "Student", typeof(StudentController) },
            { "Course", typeof(CourseController) }
        };

        public MasterApiController(IStudentServices studentServices, IJwtService jwtService)
        {
            _studentServices = studentServices;
            _jwtService = jwtService;
            this._response = new();
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("{token}/{controllerName}/{methodName}")]
        public ActionResult<APIResponse> CallExternalGetMethod(ApiRequest apiRequest)
        {
            try
            {
                var header = this.Request.Headers;
                Console.WriteLine(header);
                if (!_jwtService.ValidateToken(header["token"], out JwtSecurityToken jwtSecurityToken))
                {
                    _response.ErroMessages = new List<string> { "Token Not Validate" };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return _response;
                }
                else
                {

                    object controller = null;
                    if (controllers.TryGetValue(apiRequest.ControllerName, out Type controllerType))
                    {
                        controller = Activator.CreateInstance(controllerType, _studentServices, _jwtService);
                    }
                    MethodInfo methodInfo = controller.GetType().GetMethod(apiRequest.MethodName);
                    if (methodInfo != null)
                    {
                        //ParameterInfo[] parameters = methodInfo.GetParameters();
                        //object[] paramValues = new object[parameters.Length];

                        object newobj = ((JObject)apiRequest.DataObject).ToObject<int>();
                        var result = methodInfo.Invoke(controller, new object[] { newobj });
                        var actionResult = (ActionResult<APIResponse>)result;
                        _response = actionResult.Value;
                        return _response;

                    }
                    else
                    {
                        _response.ErroMessages = new List<string> { "Method Or Controller Invalid" };
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.NotFound;
                        return _response;
                    }
                }
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return _response;
            }

        }


    }
}
