using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;
using Microsoft.Data.SqlClient;
using System.Net;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
using System.Runtime.CompilerServices;
using StudentManagement.Models;
using System.IdentityModel.Tokens.Jwt;

namespace StudentManagment.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly IBaseServices _baseServices;
        public LoginController(IConfiguration configuration, IBaseServices baseServices)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
            _baseServices = baseServices;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckLogin(StudentViewModel studentViewModel)
        {
            try
            {
                Student student = _baseServices.CheckLoginDetails(studentViewModel);
                if (!string.IsNullOrEmpty(student.UserName))
                {
                    HttpContext.Session.SetInt32("UserId", student.StudentId);
                    HttpContext.Session.SetString("Jwt", student.JwtToken);
                    HttpContext.Session.SetInt32("RoleId", 3);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["error"] = "Invalid Username or Password";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
        }

        public IActionResult Logout()
        {
            string JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            int Id = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (HttpContext.Session.GetString("Role") != null)
            {
                _baseServices.UpdateProfessorHodJwtToken("", Id, JwtToken);
                HttpContext.Session.Clear();
                return RedirectToAction("ProfessorHodLogin", "Login");
            }
            bool isUpdate = _baseServices.UpdateJwtToken("", Id,JwtToken);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult ProfessorHodLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckAdminLogin(AdminViewModel adminViewModel)
        {
            try
            {
                ProfessorHod professorHod = _baseServices.CheckAdminLoginDetails(adminViewModel);
                if (!string.IsNullOrEmpty(professorHod.UserName))
                {
                    HttpContext.Session.SetInt32("UserId", professorHod.Id);
                    HttpContext.Session.SetInt32("RoleId", professorHod.RoleId);
                    HttpContext.Session.SetString("Jwt", professorHod.JwtToken);

                    return RedirectToAction("AdminIndex", "Home");
                }
                else
                {
                    TempData["error"] = "Invalid Username or Password";
                    return View("ProfessorHodLogin");
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
        }
    }
}
