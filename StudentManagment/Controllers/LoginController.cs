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
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;

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

        public IActionResult Logout(bool? isComeFromError)
        {
            string JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            int Id = HttpContext.Session.GetInt32("UserId") ?? 0;
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (Id != 0 && isComeFromError == null || isComeFromError != true)
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
                    if (jwtClaimsViewModel.IsPasswordUpdated == false && jwtClaimsViewModel.Id == 0)
                    {
                        string email = jwtClaimsViewModel.Email;
                        DateTime expireTime = DateTime.Now.AddHours(3);
                        string passwordToken = Crypto.HashPassword(email);
                        return RedirectToAction("EmailForgotPassword", new { token = passwordToken, ExpireTime = expireTime.ToString("dd/MM/yyyy HH:mm:ss"), username = email, IsFirstTime = true });
                    }
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
                    HttpContext.Session.SetString("UserName", jwtClaimsViewModel.UserName);
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
            var ExistingUsername = roleBaseResponse2.data.UserName;
            if (ExistingEmail != null || ExistingUsername != null)
            {
                return RedirectToAction("SignUp");
            }
            signUpViewModel.IsConfirmed = false;
            signUpViewModel.IsRejected = false;
            signUpViewModel.IsPasswordUpdated = true;
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "CreateStudent",
                DataObject = JsonConvert.SerializeObject(signUpViewModel),
            };
            _ = CallApiWithoutToken<bool>(secondApiRequest);
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckUserName(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "CheckExistingUserNamePassword",
                DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel.UserName),
            };
            RoleBaseResponse<ForgotPasswordViewModel> roleBaseResponse = CallApiWithoutToken<ForgotPasswordViewModel>(newSecondApiRequest);
            if (roleBaseResponse.data.Email == null)
            {
                TempData["error"] = "Username or Email not exist";
            }
            else
            {
                EmailViewModel emailViewModel = new();
                emailViewModel.Email = roleBaseResponse.data.Email;
                emailViewModel.Subject = "Forgot Password Link";
                string mailbody = "Please <a href=" + Url.Action("EmailForgotPassword", "Login", new { token = roleBaseResponse.data.ResetToken, ExpireTime = roleBaseResponse.data.ExpirationTime?.ToString("dd/MM/yyyy HH:mm:ss"), username = roleBaseResponse.data.Email }, "https") + ">Forgot Password</a>";
                emailViewModel.Body = mailbody;
                emailViewModel.SentBy = 1;
                SecondApiRequest SecondApiRequest = new()
                {
                    ControllerName = "Student",
                    MethodName = "SendForgotPasswordEmail",
                    DataObject = JsonConvert.SerializeObject(emailViewModel),
                };
                RoleBaseResponse<bool> roleBaseResponse1 = CallApiWithoutToken<bool>(SecondApiRequest);
                if (roleBaseResponse1.data == true)
                {
                    TempData["Success"] = "ForgotPassword link sent to your Email";
                }
                else
                {
                    TempData["error"] = "Something went wrong try after sometimes";

                }
            }
            return View("ForgotPassword");
        }

        public IActionResult EmailForgotPassword(string token, string ExpireTime, string username, bool? IsFirstTime)
        {
            if (token != null && username != null)
            {
                if (Crypto.VerifyHashedPassword(token, username) == true)
                {
                    if (DateTime.Parse(ExpireTime) < DateTime.Now)
                    {
                        TempData["error"] = "Token Expired Generate new Link";
                        return View("ForgotPassword");
                    }
                    ForgotPasswordViewModel forgotPasswordViewModel = new()
                    {
                        UserName = username,
                        ExpirationTime = DateTime.Parse(ExpireTime),
                        IsFirstTime = IsFirstTime ?? false,
                    };
                    return View(forgotPasswordViewModel);
                }
                else
                {
                    TempData["error"] = "Something went wrong try again after sometimes";
                    return View("ForgotPassword");
                }
            }
            else
            {
                TempData["error"] = "Something went wrong try again after sometimes";
                return View("ForgotPassword");
            }

        }

        [HttpPost]
        public IActionResult ChangePassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (forgotPasswordViewModel.ExpirationTime < DateTime.Now)
            {
                TempData["error"] = "Token Expired Generate new Link";
                return View("ForgotPassword");
            }

            if(forgotPasswordViewModel.IsFirstTime != true)
            {
                SecondApiRequest secondApiRequest1 = new()
                {
                    ControllerName = "Student",
                    MethodName = "CheckPreviousPasswords",
                    DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel),
                };

                RoleBaseResponse<bool> roleBaseResponse = CallApiWithoutToken<bool>(secondApiRequest1);
                if (roleBaseResponse.data == false)
                {
                    TempData["error"] = "Password is not same as Previous 3 passwords";
                    return View("ForgotPassword");
                }

            }

            SecondApiRequest SecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "ChangePassword",
                DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel),
            };
            RoleBaseResponse<bool> roleBaseResponse1 = CallApiWithoutToken<bool>(SecondApiRequest);
            if (roleBaseResponse1.data == true)
            {
                TempData["Success"] = "Password Successfully Changed";
            }
            else
            {
                TempData["error"] = "Something went wrong try after sometimes";

            }
            return View("Login");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            forgotPasswordViewModel.StudentId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "CheckPassword",
                DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel),
                MethodType = "IsViewed",
                PageName = "EditStudent",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };

            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
            if (roleBaseResponse.data == false)
            {
                TempData["error"] = "Old Password is Incorrect";
                return View();
            }

            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Student",
                MethodName = "CheckPreviousPasswords",
                DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel),
                MethodType = "IsViewed",
                PageName = "EditStudent",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };

            RoleBaseResponse<bool> roleBaseResponse1 = GetApiResponse<bool>(secondApiRequest1);
            if (roleBaseResponse1.data == false)
            {
                TempData["error"] = "Password is not same as Previous 3 passwords";
                return View();
            }

            SecondApiRequest secondApiRequest2 = new()
            {
                ControllerName = "Student",
                MethodName = "ChangePasswordById",
                DataObject = JsonConvert.SerializeObject(forgotPasswordViewModel),
                MethodType = "IsViewed",
                PageName = "EditStudent",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };

            RoleBaseResponse<bool> roleBaseResponse2 = GetApiResponse<bool>(secondApiRequest2);
            if (roleBaseResponse2.data == true)
            {
                TempData["Success"] = "Password Successfully Changed";
            }
            else
            {
                TempData["error"] = "Something went wrong try after sometimes";

            }
            return View("ResetPassword");
        }
    }
}
