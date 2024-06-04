using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;
using Microsoft.Data.SqlClient;
using System.Net;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
using System.Runtime.CompilerServices;

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
                    HttpContext.Session.SetInt32("StudentId", student.StudentId);
                    HttpContext.Session.SetString("Jwt", student.JwtToken);
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
            bool isUpdate = _baseServices.UpdateJwtToken("", HttpContext.Session.GetInt32("StudentId") ?? 0);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
