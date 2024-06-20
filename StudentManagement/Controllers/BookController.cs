using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement_API.Models;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtServices _jwtService;
        private readonly IProfessorHodServices _professorHodServices;

        public BookController(IStudentServices studentServices, IJwtServices jwtService, IProfessorHodServices professorHodServices)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices = professorHodServices;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public ActionResult<APIResponse> GetAllBooks(PaginationDto paginationDto)
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
                IList<Book> books = _studentServices.GetBooksWithPegination(paginationDto);
                int totalItems = books.Count > 0 ? books.FirstOrDefault(x => x.BookId != 0)?.TotalRecords ?? 0 : 0;
                int TotalPages = (int)Math.Ceiling((decimal)totalItems / paginationDto.PageSize);
                RoleBaseResponse<Book> roleBaseResponse = new()
                {
                    data = books,
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> CreateBook([FromBody] Book book)
        {
            try
            {

                _studentServices.InsertBook(book);
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


        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<APIResponse> UpdateBook([FromBody] Book book)
        {
            try
            {

                _studentServices.UpdateBook(book);
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
