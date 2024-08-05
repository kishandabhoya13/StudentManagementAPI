using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.Net;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtServices _jwtService;
        private readonly IProfessorHodServices _professorHodServices;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private readonly IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();


        public CurrencyController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices,
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
        [HttpGet("GetCurrencyPairRate")]
        public ActionResult<APIResponse> GetCurrencyPairRate(CurrencyPairDto currencyPairDto)
        {
            try
            {
                CurrencyPairDto currencyPairDto1 = _studentServices.GetCurrencyPairData(currencyPairDto.CurrencyPair);
                RoleBaseResponse<CurrencyPairDto> roleBaseResponse = new()
                {
                    data = currencyPairDto1,
                };
                _response.result = roleBaseResponse;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
            }

            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpsertRateAlert")]
        public ActionResult<APIResponse> UpsertRateAlert(CurrencyPairDto currencyPairDto)
        {
            try
            {
                _studentServices.UpsertRateAlert(currencyPairDto);
                _response.result = new RoleBaseResponse<bool>() { data = true };
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch(Exception ex) 
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetRateAlerts")]
        public ActionResult<APIResponse> GetRateAlerts(int StudentId)
        {
            try
            {
                IList<CurrencyPairDto> currencyPairDto = _studentServices.GetRateAlerts(StudentId);
                RoleBaseResponse<IList<CurrencyPairDto>> roleBaseResponse = new()
                {
                    data = currencyPairDto
                };
                _response.result = roleBaseResponse;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
            }
            return _response;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetRateAlertById")]
        public ActionResult<APIResponse> GetRateAlertById(int RateAlertId)
        {
            try
            {
                CurrencyPairDto currencyPairDto = _studentServices.GetRateAlertById(RateAlertId);
                RoleBaseResponse<CurrencyPairDto> roleBaseResponse = new()
                {
                    data = currencyPairDto
                };
                _response.result = roleBaseResponse;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
            }
            return _response;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("RemoveRateAlert")]
        public ActionResult<APIResponse> RemoveRateAlert(int RateAlertId)
        {
            try
            {
                _studentServices.RemoveRateAlert(RateAlertId);
                RoleBaseResponse<bool> roleBaseResponse = new()
                {
                    data = true
                };
                _response.result = roleBaseResponse;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                _response.ErroMessages = new List<string> { ex.Message };
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Unauthorized;
            }
            return _response;
        }
    }
}
