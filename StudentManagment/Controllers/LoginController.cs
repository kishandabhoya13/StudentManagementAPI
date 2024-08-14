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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using StudentManagement.Models.DTO;
using System.Net.Http;
using System.Web.Helpers;

namespace StudentManagment.Controllers
{
    public class LoginController : MasterController
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

        //[HttpPost]
        //public IActionResult CheckLogin(StudentViewModel studentViewModel)
        //{
        //    try
        //    {
        //        Student student = _baseServices.CheckLoginDetails(studentViewModel);
        //        if (!string.IsNullOrEmpty(student.UserName))
        //        {
        //            HttpContext.Session.SetInt32("UserId", student.StudentId);
        //            HttpContext.Session.SetString("Jwt", student.JwtToken);
        //            HttpContext.Session.SetInt32("RoleId", 3);

        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            TempData["error"] = "Invalid Username or Password";
        //            return View("Login");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var message = ex.Message;
        //        throw;
        //    }
        //}

        public IActionResult Logout()
        {
            string JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            int Id = HttpContext.Session.GetInt32("UserId") ?? 0;
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (Id != 0)
            {
                UpdateJwtViewModel updateJwtViewModel = new()
                {
                    Id = Id,
                    token = "",
                };
                if (RoleId != 3)
                {

                    SecondApiRequest secondApiRequest = new()
                    {
                        ControllerName = "ProfessorHod",
                        MethodName = "UpdateProfessorHodJwtToken",
                        DataObject = JsonConvert.SerializeObject(updateJwtViewModel),
                        RoleIds = new List<string> { "1", "2" },
                        token = JwtToken,

                    };
                    _ = GetApiResponse<bool>(secondApiRequest);
                }
                else
                {
                    SecondApiRequest secondApiRequest = new()
                    {
                        ControllerName = "Student",
                        MethodName = "UpdateStudentJwtToken",
                        DataObject = JsonConvert.SerializeObject(updateJwtViewModel),
                        RoleIds = new List<string> { "1", "2", "3" },
                        token = JwtToken,

                    };
                    _ = GetApiResponse<bool>(secondApiRequest);
                }
            }

            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Delete("jwt");
            HttpContext.Request.Headers.Remove("Api-Version");
            return RedirectToAction("Login");
        }

        public IActionResult ProfessorHodLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckLogin(AdminStudentViewModel adminViewModel)
        {
            try
            {
                StudentLoginViewModel studentLoginViewModel = new()
                {
                    UserName = adminViewModel.UserName,
                    Password = adminViewModel.Password,
                };
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Login",
                    MethodName = "CheckLoginDetails",
                    DataObject = JsonConvert.SerializeObject(studentLoginViewModel),

                };
                RoleBaseResponse<JwtClaimsViewModel> roleBaseResponse = CallApiWithoutToken<JwtClaimsViewModel>(secondApiRequest);
                JwtClaimsViewModel jwtClaimsViewModel = roleBaseResponse.data;
                if (jwtClaimsViewModel != null && !string.IsNullOrEmpty(jwtClaimsViewModel.UserName))
                {
                    if (jwtClaimsViewModel.IsBlocked == true)
                    {
                        TempData["error"] = "You are blocked by Hod";
                        return View("Login");
                    }
                    if (jwtClaimsViewModel.RoleId == 0)
                    {

                        HttpContext.Session.SetInt32("RoleId", 3);
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("RoleId", jwtClaimsViewModel.RoleId);
                    }
                    HttpContext.Session.SetString("Jwt", jwtClaimsViewModel.JwtToken);
                    if (jwtClaimsViewModel.RoleId == 3)
                    {
                        HttpContext.Session.SetString("Email", jwtClaimsViewModel.Email);
                    }

                    HttpContext.Session.SetString("FullName", jwtClaimsViewModel.FirstName + " " + jwtClaimsViewModel.LastName);
                    Response.Cookies.Append("jwt", jwtClaimsViewModel.JwtToken);

                    if (jwtClaimsViewModel.Id != 0)
                    {
                        HttpContext.Session.SetInt32("UserId", jwtClaimsViewModel.Id);
                        return RedirectToAction("Dashboard", "Home");
                    }
                    else
                    {
                        if (jwtClaimsViewModel.IsConfirmed == false || jwtClaimsViewModel.IsRejected == true)
                        {
                            TempData["error"] = "Your Request is not Approved by HOD";
                            return View("Login");
                        }
                        HttpContext.Session.SetInt32("UserId", jwtClaimsViewModel.StudentId);
                        return RedirectToAction("Index", "Home");

                    }
                }
                else
                {
                    TempData["error"] = "Invalid Username/Password";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw;
            }
        }


        public IActionResult SignUp()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            RoleBaseResponse<IList<Course>> roleBaseResponse1 = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetAllCourses",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllCourses",
            };

            roleBaseResponse1 = CallApiWithoutToken<IList<Course>>(secondApiRequest);
            SignUpViewModel signUpViewModel = new()
            {
                Courses = roleBaseResponse1.data,
            };
            return View(signUpViewModel);
        }

        [HttpPost]
        public IActionResult SignUp(SignUpViewModel signUpViewModel)
        {
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudentByEmail",
                DataObject = JsonConvert.SerializeObject(signUpViewModel.Email),
            };
            RoleBaseResponse<StudentViewModel> roleBaseResponse = CallApiWithoutToken<StudentViewModel>(newSecondApiRequest);
            var ExistingEmail = roleBaseResponse.data.Email;

            SecondApiRequest newSecondApiRequest2 = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudentByUserName",
                DataObject = JsonConvert.SerializeObject(signUpViewModel.UserName),
            };
            RoleBaseResponse<StudentViewModel> roleBaseResponse2 = CallApiWithoutToken<StudentViewModel>(newSecondApiRequest2);
            var ExistingUsername = roleBaseResponse.data.UserName;
            if (ExistingEmail != null || ExistingUsername != null)
            {
                return RedirectToAction("SignUp");
            }
            signUpViewModel.IsConfirmed = false;
            signUpViewModel.IsRejected = false;
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "CreateStudent",
                DataObject = JsonConvert.SerializeObject(signUpViewModel),
            };
            _ = CallApiWithoutToken<bool>(secondApiRequest);
            return RedirectToAction("Login");
        }
    }
}
