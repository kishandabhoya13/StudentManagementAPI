using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private readonly IProfessorHodServices _professorHodServices;
        private APIResponse _response;
        public Dictionary<string, Type> controllers = new()
        {
            { "Student", typeof(StudentController) },
            { "Course", typeof(CourseController) },
            { "ProfessorHod", typeof(ProfessorHodController) }
        };

        public MasterApiController(IStudentServices studentServices, IJwtService jwtService, IProfessorHodServices professorHodServices)
        {
            _studentServices = studentServices;
            _jwtService = jwtService;
            this._response = new();
            _professorHodServices = professorHodServices;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status203NonAuthoritative)]
        [HttpPost("{controllerName}/{methodName}")]
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
                    bool isAuthorized = false;
                    if (apiRequest.MethodName == "UpdateStudentJwtToken" || apiRequest.MethodName == "UpdateProfessorHodJwtToken")
                    {
                        isAuthorized = true;
                    }
                    else
                    {
                        isAuthorized = _professorHodServices.IsAuthorized(apiRequest);

                    }
                    if (isAuthorized)
                    {
                        object controller = null;
                        if (controllers.TryGetValue(apiRequest.ControllerName, out Type controllerType))
                        {
                            controller = Activator.CreateInstance(controllerType, _studentServices, _jwtService, _professorHodServices);
                        }
                        MethodInfo methodInfo = controller.GetType().GetMethod(apiRequest.MethodName);
                        if (methodInfo != null)
                        {
                            if (apiRequest.DataObject != "null")
                            {
                                object dtoObject = JsonConvert.DeserializeObject<dynamic>(apiRequest.DataObject);

                                var value = _studentServices.GetDynamicData(apiRequest.ControllerName, apiRequest.MethodName, dtoObject);
                                var result = methodInfo.Invoke(controller, new object[] { value });
                                var actionResult = (ActionResult<APIResponse>)result;
                                _response = actionResult.Value;
                                return _response;
                            }
                            else
                            {
                                if (apiRequest.MethodName == "GetAllStudents")
                                {
                                    var result = methodInfo.Invoke(controller, header["token"]);
                                    var actionResult = (ActionResult<APIResponse>)result;
                                    _response = actionResult.Value;
                                }
                                else
                                {
                                    var result = methodInfo.Invoke(controller, null);
                                    var actionResult = (ActionResult<APIResponse>)result;
                                    _response = actionResult.Value;
                                }
                                return _response;
                            }
                        }
                        else
                        {
                            _response.ErroMessages = new List<string> { "Method Or Controller Invalid" };
                            _response.IsSuccess = false;
                            _response.StatusCode = HttpStatusCode.NotFound;
                            return _response;
                        }
                    }
                    else
                    {
                        _response.ErroMessages = new List<string> { "Not Authoried " };
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.NonAuthoritativeInformation;
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("{controllerName}/{methodName}/Login")]
        public ActionResult<APIResponse> CallExternamLoginMethod(ApiRequest apiRequest)
        {
            try
            {
                object controller = null;
                if (controllers.TryGetValue(apiRequest.ControllerName, out Type controllerType))
                {
                    controller = Activator.CreateInstance(controllerType, _studentServices, _jwtService, _professorHodServices);
                }
                MethodInfo methodInfo = controller.GetType().GetMethod(apiRequest.MethodName);
                if (methodInfo != null)
                {
                    object dtoObject = JsonConvert.DeserializeObject<dynamic>(apiRequest.DataObject);

                    //var newobj = ((JObject)dtoObject).ToObject<StudentLoginDto>();
                    var value = _studentServices.GetDynamicData(apiRequest.ControllerName, apiRequest.MethodName, dtoObject);
                    //int intValue = Convert.ToInt32(dtoObject);
                    var result = methodInfo.Invoke(controller, new object[] { value });
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
