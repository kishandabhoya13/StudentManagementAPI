using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Net;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtServices _jwtService;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public LoginController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices,
            IConfiguration configuration, IMapper mapper)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices = professorHodServices;
            _configuration = configuration;
            _mapper = mapper;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("CheckLoginDetails", Name = "CheckLoginDetails")]
        public ActionResult<APIResponse> CheckLoginDetails([FromQuery] StudentLoginDto studentLoginDto)
        {
            try
            {
                JwtClaimsDto jwtClaimsDto1 = _studentServices.GetLoginStudentDetails(studentLoginDto);
                if (jwtClaimsDto1 != null && jwtClaimsDto1.StudentId != 0)
                {
                    jwtClaimsDto1.RoleId = 3;
                    _response.result = jwtClaimsDto1;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return _response;
                }
                else
                {
                    JwtClaimsDto jwtClaimsDto = _professorHodServices.CheckUserNamePassword(studentLoginDto);
                    _response.result = jwtClaimsDto;
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return _response;
                }

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
        [HttpGet("GetApiVersion")]
        public ActionResult<SettingDto> GetApiVersion()
        {
            try
            {
                SettingDto settingDto= _studentServices.GetApiVersion();
                return settingDto;

            }
            catch (Exception ex)
            {
                return new SettingDto();
            }
        }
    }
}
