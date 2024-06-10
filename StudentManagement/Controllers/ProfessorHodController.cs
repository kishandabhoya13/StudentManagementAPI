﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement_API.Models;
using StudentManagement_API.Models.DTO;
using StudentManagement_API.Services.Interface;
using StudentManagment_API.Services.Interface;
using System.Net;

namespace StudentManagement_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessorHodController : ControllerBase
    {
        private APIResponse _response;
        private readonly IStudentServices _studentServices;
        private readonly IJwtService _jwtService;
        private readonly IProfessorHodServices _professorHodServices;
        public ProfessorHodController(IStudentServices studentServices, IJwtService jwtService,IProfessorHodServices? professorHodServices)
        {
            this._response = new();
            _studentServices = studentServices;
            _jwtService = jwtService;
            _professorHodServices= professorHodServices;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("CheckLogin", Name = "LoginHodDetails")]
        public ActionResult<APIResponse> LoginDetails([FromQuery] StudentLoginDto studentLoginDto)
        {
            try
            {
                ProfessorHod professorHod= _professorHodServices.CheckUserNamePassword(studentLoginDto);
                _response.result = professorHod;
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

    }
}
