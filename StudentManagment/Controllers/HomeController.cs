using AutoMapper;
using AutoMapper.Internal;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Drawing.Chart.Style;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Crypto.Modes;
using SelectPdf;
using StudentManagement.Models;
using StudentManagement.Models.DTO;
using StudentManagement_API.Models.Models.DTO;
using StudentManagement_API.Services.CacheService;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services;
using StudentManagment.Services.Interface;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Helpers;
using System.Web.Razor.Tokenizer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace StudentManagment.Controllers
{

    public class HomeController : MasterController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseServices _baseServices;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ICacheServices _cacheServices;
        private static readonly Random _random = new Random();


        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, IBaseServices baseServices,
            IMapper mapper, IConfiguration configuration, ICacheServices cacheServices, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _baseServices = baseServices;
            _mapper = mapper;
            _cacheServices = cacheServices;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {

            int StudentId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (StudentId == 0)
            {
                return RedirectToAction("Login", "Login");
            }
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 3)
            {
                return View("NotAuthorized");
            }
            string token = HttpContext.Session.GetString("Jwt");
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudent",
                DataObject = JsonConvert.SerializeObject(StudentId.ToString()),
                MethodType = "IsViewed",
                PageName = "EditStudent",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2", "3" },
                token = token,
            };
            RoleBaseResponse<Student> roleBaseResponse = GetApiResponse<Student>(secondApiRequest);
            Student student = roleBaseResponse.data;
            StudentViewModel studentViewModel = _mapper.Map<StudentViewModel>(student);
            return View(studentViewModel);
        }

        public IActionResult AdminIndex()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1 && RoleId != 2)
            {
                return View("NotAuthorized");
            }
            if (RoleId == 2)
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }

            IList<Course> courses = _cacheServices.GetListCachedResponse<Course>("Courses");
            RoleBaseResponse<IList<Course>> roleBaseResponse1 = new();
            if (courses == null || courses.Count == 0)
            {
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Course",
                    MethodName = "GetAllCourses",
                    DataObject = JsonConvert.SerializeObject(null),
                    MethodType = "IsViewed",
                    PageName = "GetAllCourses",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1", "2" },
                    token = token,
                };

                roleBaseResponse1 = GetApiResponse<IList<Course>>(secondApiRequest);
            }


            RoleBaseResponse<Student> roleBaseResponse = new()
            {
                Courses = roleBaseResponse1.data
            };
            if (roleBaseResponse.IsAuthorize == false)
            {
                return RedirectToAction("Logout", "Login");
            }
            return View(roleBaseResponse);
        }

        [HttpPost]
        public IActionResult AdminIndexTableView(SecondApiRequest secondApiRequest)
        {
            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
                JwtToken = secondApiRequest.token,
                FromDate = secondApiRequest.FromDate,
                ToDate = secondApiRequest.ToDate
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllStudents",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = secondApiRequest.token,


            };
            int? role = HttpContext.Session.GetInt32("RoleId");
            RoleBaseResponse<IList<Student>> roleBaseResponse = GetApiResponse<IList<Student>>(newSecondApiRequest);
            foreach (var data in roleBaseResponse.data)
            {
                data.currentUserRole = role;
            }
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);
        }


        [HttpGet]
        public IActionResult GetStudentProfessorChartDetails(int month, int year)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            CountStudentProfessor countStudentProfessor = new()
            {
                month = month,
                year = year,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "DayWiseCountStudentProf",
                DataObject = JsonConvert.SerializeObject(countStudentProfessor),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,

            };
            RoleBaseResponse<IList<CountStudentProfessor>> roleBaseResponse = GetApiResponse<IList<CountStudentProfessor>>(secondApiRequest);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            string abbreviatedMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            roleBaseResponse.MonthName = abbreviatedMonthName;
            roleBaseResponse.year = year;
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime currentDate = startDate;
            if (roleBaseResponse.data != null && roleBaseResponse.data.Count > 0)
            {
                while (currentDate <= endDate)
                {
                    CountStudentProfessor countEmailViewModel = new()
                    {
                        CreatedDate1 = currentDate,
                        CreatedDate2 = currentDate,
                        ProfessorDayWiseCount = 0,
                        StudentDayWiseCount = 0,
                    };
                    ((IList<CountStudentProfessor>)roleBaseResponse.data).Add(countEmailViewModel);
                    currentDate = currentDate.AddDays(1);
                }
            }

            return Json(roleBaseResponse);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CreateUpdateStudent(int? StudentId)
        {
            StudentViewModel studentViewModel = new();
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            if ((StudentId == null && RoleId == 3) || (StudentId == null && RoleId == 2))
            {
                return View("NotAuthorized");
            }
            if (StudentId != null && StudentId != 0)
            {
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Student",
                    MethodName = "GetStudent",
                    DataObject = JsonConvert.SerializeObject(StudentId.ToString()),
                    MethodType = "IsManaged",
                    PageName = "EditStudent",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1", "2", "3" },
                    token = token
                };
                RoleBaseResponse<Student> roleBaseResponse1 = GetApiResponse<Student>(secondApiRequest);
                Student student = roleBaseResponse1.data;
                studentViewModel = _mapper.Map<StudentViewModel>(student);
            }
            RoleBaseResponse<IList<Course>> roleBaseResponse = new();
            SecondApiRequest CourseecondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetAllCourses",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllCourses",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2", "3" },
                token = token,
            };

            roleBaseResponse = GetApiResponse<IList<Course>>(CourseecondApiRequest);
            if (roleBaseResponse.IsAuthorize == false)
            {
                return RedirectToAction("Logout", "Login");
            }
            studentViewModel.Courses = roleBaseResponse.data;
            return View(studentViewModel);
        }

        [HttpPost]
        public IActionResult UpsertStudent(StudentViewModel studentViewModel)
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            studentViewModel.RoleId = HttpContext.Session.GetInt32("RoleId");
            SecondApiRequest secondApiRequest = new();
            studentViewModel.IsConfirmed = true;
            studentViewModel.IsRejected = false;
            if (studentViewModel.StudentId != 0)
            {
                secondApiRequest.ControllerName = "Student";
                secondApiRequest.MethodName = "UpdateStudent";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(studentViewModel);
                secondApiRequest.PageName = "EditStudent";
                secondApiRequest.MethodType = "IsManaged";
                secondApiRequest.RoleId = studentViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1", "2", "3" };
                secondApiRequest.token = token;

            }
            else
            {
                secondApiRequest.ControllerName = "Student";
                secondApiRequest.MethodName = "CreateStudent";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(studentViewModel);
                secondApiRequest.PageName = "CreateStudent";
                secondApiRequest.MethodType = "IsInsert";
                secondApiRequest.RoleId = studentViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1", "2" };
                secondApiRequest.token = token;

            }
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
            //bool isSuccess = _baseServices.UpsertStudent(studentViewModel);
            if (roleBaseResponse.data)
            {
                if (HttpContext.Session.GetString("Role") != null)
                {
                    return RedirectToAction("AdminIndex", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["ErrorMsg"] = "Something wents Wrong try again after Sometimes";
                return RedirectToAction("CreateUpdateStudent");
            }
        }

        public IActionResult CreateCourseModal()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddCourse(Course course)
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "CreateCourse",
                DataObject = JsonConvert.SerializeObject(course),
                PageName = "CreateCourse",
                MethodType = "IsInsert",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
            //bool isSuccess = _baseServices.InsertCouse(course, RoleId);
            if (roleBaseResponse.data)
            {
                return RedirectToAction("AdminIndex", "Home");
            }
            else
            {
                return View("CreateCourseModal");
            }
        }

        public IActionResult AllBooks()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetAllCourses",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllCourses",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<IList<Course>> roleBaseResponse1 = GetApiResponse<IList<Course>>(secondApiRequest);
            if (roleBaseResponse1.IsAuthorize == false)
            {
                return RedirectToAction("Logout", "Login");
            }
            RoleBaseResponse<Book> roleBaseResponse = new()
            {
                Courses = roleBaseResponse1.data,
            };
            //roleBaseResponse = _baseServices.GetAllStudentsWithPagination(secondApiRequest);
            return View(roleBaseResponse);
        }

        [HttpPost]
        public IActionResult GetFilteredBooks(SecondApiRequest secondApiRequest)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            //if (RoleId != 1)
            //{
            //    return View("NotAuthorized");
            //}
            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";


            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
                JwtToken = secondApiRequest.token
            };
            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Book",
                MethodName = "GetAllBooks",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllBooks",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1" },
                token = secondApiRequest.token

            };
            RoleBaseResponse<IList<Book>> roleBaseResponse = GetApiResponse<IList<Book>>(secondApiRequest1);
            //foreach(Book book in roleBaseResponse.data)
            //{
            //    if(book.Photo != null)
            //    {
            //        byte[] bytedata = book.Photo;
            //        string data = Convert.ToBase64String(bytedata);
            //        book.Photos = string.Format("data:image/png;base64,{0}", data);
            //    }

            //}
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);

        }


        public IActionResult AddEditBook(int BookId)
        {
            BookViewModel bookViewModel = new();
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt");
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            if (BookId != 0)
            {
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Book",
                    MethodName = "GetBook",
                    DataObject = JsonConvert.SerializeObject(BookId.ToString()),
                    MethodType = "IsViewed",
                    PageName = "GetAllBooks",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token
                };
                //Book book = _baseServices.GetBook(BookId ?? 0, token, secondApiRequest);
                RoleBaseResponse<Book> roleBaseResponse = GetApiResponse<Book>(secondApiRequest);
                if (roleBaseResponse.IsAuthorize == false)
                {
                    return RedirectToAction("Logout", "Login");
                }
                bookViewModel = _mapper.Map<BookViewModel>(roleBaseResponse.data);
                if (roleBaseResponse.data.Photo != null)
                {
                    using var stream = new MemoryStream(roleBaseResponse.data.Photo);

                    IFormFile file = new FormFile(stream, 0, roleBaseResponse.data.Photo.Length, "name", "fileName");
                    bookViewModel.PhotoFile = file;
                }

            }
            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Course",
                MethodName = "GetAllCourses",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllCourses",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<IList<Course>> roleBaseResponse1 = GetApiResponse<IList<Course>>(secondApiRequest1);
            if (roleBaseResponse1.IsAuthorize == false)
            {
                return RedirectToAction("Logout", "Login");
            }
            bookViewModel.Courses = roleBaseResponse1.data;
            return View(bookViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertBook(BookViewModel bookViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            bookViewModel.JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            bookViewModel.RoleId = HttpContext.Session.GetInt32("RoleId");
            if (bookViewModel.PhotoFile != null)
            {
                IFormFile SingleFile = bookViewModel.PhotoFile;
                string fileName = Guid.NewGuid().ToString() + Path.GetFileName(SingleFile.FileName);
                bookViewModel.PhotoName = fileName;
                using var memoryStream = new MemoryStream();
                await SingleFile.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                bookViewModel.Photo = imageData;
            }
            SecondApiRequest secondApiRequest = new();
            if (bookViewModel.BookId != 0)
            {
                secondApiRequest.ControllerName = "Book";
                secondApiRequest.MethodName = "UpdateBook";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(bookViewModel);
                secondApiRequest.PageName = "UpsertBook";
                secondApiRequest.MethodType = "IsManaged";
                secondApiRequest.RoleId = bookViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1" };
                secondApiRequest.token = bookViewModel.JwtToken;
            }
            else
            {
                secondApiRequest.ControllerName = "Book";
                secondApiRequest.MethodName = "CreateBook";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(bookViewModel);
                secondApiRequest.PageName = "UpsertBook";
                secondApiRequest.MethodType = "IsInsert";
                secondApiRequest.RoleId = bookViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1" };
                secondApiRequest.token = bookViewModel.JwtToken;
            }

            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
            if (roleBaseResponse.data)
            {
                return RedirectToAction("AllBooks", "Home");
            }
            else
            {
                TempData["ErrorMsg"] = "Something wents Wrong try again after Sometimes";
                return RedirectToAction("CreateUpdateStudent");
            }
        }

        [HttpGet]
        public IActionResult BookDeleteModal(int BookId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            BookViewModel bookViewModel = new()
            {
                BookId = BookId,
            };

            return View(bookViewModel);
        }

        public IActionResult DeleteBook(int BookId)
        {
            BookViewModel bookViewModel = new()
            {
                BookId = BookId,
                JwtToken = HttpContext.Session.GetString("Jwt") ?? "",
                RoleId = HttpContext.Session.GetInt32("RoleId"),
            };

            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Book",
                MethodName = "DeleteBook",
                DataObject = JsonConvert.SerializeObject(bookViewModel),
                PageName = "DeleteBook",
                MethodType = "IsManaged",
                RoleId = bookViewModel.RoleId,
                RoleIds = new List<string> { "1" },
                token = bookViewModel.JwtToken,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
            if (roleBaseResponse.data)
            {
                return RedirectToAction("AllBooks");
            }
            TempData["Error"] = "Something went wrong Try again after Sometimes";
            return RedirectToAction("AllBooks");
        }

        [HttpGet]
        public IActionResult ViewBookPhoto(int BookId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            if (BookId != 0)
            {
                BookViewModel bookViewModel = new()
                {
                    BookId = BookId,
                    RoleId = HttpContext.Session.GetInt32("RoleId"),
                };
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Book",
                    MethodName = "GetBookPhoto",
                    DataObject = JsonConvert.SerializeObject(bookViewModel),
                    MethodType = "IsViewed",
                    PageName = "GetAllBooks",
                    RoleId = bookViewModel.RoleId,
                    RoleIds = new List<string> { "1", "2" },
                    token = HttpContext.Session.GetString("Jwt") ?? ""
                };
                RoleBaseResponse<Book> roleBaseResponse = GetApiResponse<Book>(secondApiRequest);
                if (roleBaseResponse.IsAuthorize == false)
                {
                    return RedirectToAction("Logout", "Login");
                }
                Book book = roleBaseResponse.data;
                if (book.Photo != null)
                {
                    return File(book.Photo, "image/jpeg");
                }
            }
            return NotFound();
        }

        public IActionResult AllEmails()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SendEmailModal(int ScheduledEmailId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            EmailViewModel emailViewModel = new()
            {
                StudentId = 0
            };
            if (ScheduledEmailId != 0)
            {
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Email",
                    MethodName = "GetScheduledEmailById",
                    DataObject = JsonConvert.SerializeObject(ScheduledEmailId.ToString()),
                    MethodType = "IsViewed",
                    PageName = "GetAllStudents",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token

                };
                RoleBaseResponse<EmailViewModel> roleBaseResponse = GetApiResponse<EmailViewModel>(secondApiRequest);
                if (roleBaseResponse.IsAuthorize == false)
                {
                    return Json(false);
                }
                emailViewModel = roleBaseResponse.data;
                //if (emailViewModel.AttachmentsByte != null && emailViewModel.AttachmentsByte.Count > 0)
                //{
                //    emailViewModel.AttachmentFiles = new();
                //    var tasks = emailViewModel.AttachmentsByte.Select(async attachment =>
                //    {
                //        using (var memoryStream = new MemoryStream(attachment))
                //        {
                //            try
                //            {
                //                IFormFile file = new FormFile(memoryStream, 0, attachment.Length, "name", "fileName");
                //                emailViewModel.AttachmentFiles.Add(file);
                //            }
                //            catch (Exception ex)
                //            {
                //                Console.WriteLine($"Error copying attachment: {ex.Message}");
                //            }
                //        }


                //    });
                //    await Task.WhenAll(tasks);
                //}
            }
            else
            {
                string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
                string Body = System.IO.File.ReadAllText(filePath);
                Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
                emailViewModel.Body = Body;
            }

            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Student",
                MethodName = "GetEmailsAndStudentIds",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,

            };
            RoleBaseResponse<IList<StudentsEmailAndIds>> roleBaseResponse1 = GetApiResponse<IList<StudentsEmailAndIds>>(secondApiRequest1);
            if (roleBaseResponse1.IsAuthorize == false)
            {
                return Json(false);
            }
            emailViewModel.StudentsEmails = roleBaseResponse1.data;
            return View(emailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(EmailViewModel emailViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            emailViewModel.RoleId = RoleId;
            emailViewModel.JwtToken = token;
            if (emailViewModel.ScheduledEmailId != 0)
            {
                emailViewModel.SentBy = HttpContext.Session.GetInt32("UserId");

                if (emailViewModel.AttachmentFiles != null && emailViewModel.AttachmentFiles.Count > 0)
                {
                    emailViewModel.AttachmentsByte = new();
                    var tasks = emailViewModel.AttachmentFiles.Select(async attachment =>
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            try
                            {
                                await attachment.CopyToAsync(memoryStream);
                                var imageData = memoryStream.ToArray();
                                emailViewModel.AttachmentsByte.Add(imageData);
                            }
                            catch (Exception ex)
                            {
                                // Handle or log the exception as needed
                                Console.WriteLine($"Error copying attachment: {ex.Message}");
                            }
                        }


                    });
                    await Task.WhenAll(tasks);
                }
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Email",
                    MethodName = "AddEditScheduledEmailLogs",
                    DataObject = JsonConvert.SerializeObject(emailViewModel),
                    PageName = "EmailLogs",
                    MethodType = "IsManaged",
                    RoleId = emailViewModel.RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token,
                };
                RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(secondApiRequest);
                //_baseServices.UpdateScheduledEmailLog(emailViewModel);
            }
            else
            {

                emailViewModel.SentBy = HttpContext.Session.GetInt32("UserId") ?? 0;
                emailViewModel.Emails = new();
                if (emailViewModel.StudentId == 0)
                {
                    EmailViewModel newemailViewModel = new();
                    SecondApiRequest secondApiRequest = new()
                    {
                        ControllerName = "Student",
                        MethodName = "GetEmailsAndStudentIds",
                        DataObject = JsonConvert.SerializeObject(null),
                        MethodType = "IsViewed",
                        PageName = "GetAllStudents",
                        RoleId = RoleId,
                        RoleIds = new List<string> { "1" },
                        token = token,

                    };
                    RoleBaseResponse<List<StudentsEmailAndIds>> roleBaseResponse = GetApiResponse<List<StudentsEmailAndIds>>(secondApiRequest);
                    newemailViewModel.StudentsEmails = roleBaseResponse.data;
                    if (newemailViewModel.StudentsEmails.Count > 0)
                    {
                        foreach (var studentEmailsIds in newemailViewModel.StudentsEmails)
                        {
                            emailViewModel.Emails.Add(studentEmailsIds.Email);
                        }
                    }
                }
                else
                {
                    SecondApiRequest secondApiRequest = new()
                    {
                        ControllerName = "Student",
                        MethodName = "GetEmailFromStudentId",
                        DataObject = JsonConvert.SerializeObject(emailViewModel),
                        MethodType = "IsViewed",
                        PageName = "GetAllStudents",
                        RoleId = emailViewModel.RoleId,
                        RoleIds = new List<string> { "1" },
                        token = token,
                    };
                    RoleBaseResponse<EmailViewModel> roleBaseResponse = GetApiResponse<EmailViewModel>(secondApiRequest);
                    emailViewModel.Email = roleBaseResponse.data.Email;
                    emailViewModel.Emails.Add(emailViewModel.Email);
                }
                //string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
                //string Body = System.IO.File.ReadAllText(filePath);
                //Body = Body.Replace("{{ Body }}", emailViewModel.Body);
                //Body = Body.Replace("{{ Subject }}", emailViewModel.Subject);
                //Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
                //emailViewModel.Body = Body;


                if (emailViewModel.AttachmentFiles != null && emailViewModel.AttachmentFiles.Count > 0)
                {
                    emailViewModel.FileNameWithAttachments = new();
                    var tasks = emailViewModel.AttachmentFiles.Select(async attachment =>
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            try
                            {
                                await attachment.CopyToAsync(memoryStream);
                                var imageData = memoryStream.ToArray();
                                emailViewModel.FileNameWithAttachments.Add(attachment.FileName, imageData);
                            }
                            catch (Exception ex)
                            {
                                // Handle or log the exception as needed
                                Console.WriteLine($"Error copying attachment: {ex.Message}");
                            }
                        }


                    });
                    await Task.WhenAll(tasks);
                }
                SecondApiRequest secondApiRequest1 = new()
                {
                    ControllerName = "ProfessorHod",
                    MethodName = "SendEmail",
                    DataObject = JsonConvert.SerializeObject(emailViewModel),
                    MethodType = "IsViewed",
                    PageName = "GetAllStudents",
                    RoleId = emailViewModel.RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token,
                };
                RoleBaseResponse<bool> roleBaseResponse1 = GetApiResponse<bool>(secondApiRequest1);
                //_baseServices.SendEmail(emailViewModel);
            }

            return RedirectToAction("AllEmails");
        }



        public IActionResult AllScheduledEmails()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            if (RoleId == 2)
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            //roleBaseResponse = _baseServices.GetAllStudentsWithPagination(secondApiRequest);
            return View();
        }

        [HttpPost]
        public IActionResult GetFilteredScheduledEmails(SecondApiRequest secondApiRequest)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";

            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
            };
            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Email",
                MethodName = "GetScheduledEmails",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "EmailLogs",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1" },
                token = secondApiRequest.token,

            };
            RoleBaseResponse<IList<ScheduledEmailViewModel>> roleBaseResponse = GetApiResponse<IList<ScheduledEmailViewModel>>(secondApiRequest1);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);
        }

        public IActionResult NotAuthorized()
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult TotalEmailChart()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            return View();
        }

        [HttpGet]
        public IActionResult GetChartDetails(int month, int year)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            CountEmailViewModel countEmailViewModel1 = new()
            {
                month = month,
                year = year,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Email",
                MethodName = "GetDayWiseEmailCount",
                DataObject = JsonConvert.SerializeObject(countEmailViewModel1),
                MethodType = "IsViewed",
                PageName = "EmailLogs",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token

            };
            RoleBaseResponse<IList<CountEmailViewModel>> roleBaseResponse = GetApiResponse<IList<CountEmailViewModel>>(secondApiRequest);

            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }

            string abbreviatedMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            roleBaseResponse.MonthName = abbreviatedMonthName;
            roleBaseResponse.year = year;
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime currentDate = startDate;
            if (roleBaseResponse.data != null)
            {
                while (currentDate <= endDate)
                {
                    CountEmailViewModel countEmailViewModel = new()
                    {
                        SentDate = currentDate,
                        DayWiseCount = 0,
                    };
                    ((IList<CountEmailViewModel>)roleBaseResponse.data).Add(countEmailViewModel);
                    currentDate = currentDate.AddDays(1);
                }
            }
            return Json(roleBaseResponse);
        }

        public IActionResult GetAllMailReplies()
        {
            List<EmailViewModel> emailViewModels = new();
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]), SecureSocketOptions.SslOnConnect);
                client.Authenticate(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadOnly);

                //var query = SearchQuery.HeaderContains("In-Reply-To", "<123456789@mail.example.com>");

                //var combineQuery = SearchQuery.Or(query,query1);
                var query1 = SearchQuery.SubjectContains("Re: Reply testing mail");


                var uids = inbox.Search(query1);
                foreach (var uid in uids)
                {
                    var message = inbox.GetMessage(uid);
                    string subject = message.Subject;

                    string textBody = message.TextBody;
                    string htmlBody = message.HtmlBody;
                    List<MimePart> attachments = new List<MimePart>();

                    if (message.Headers["References"].Contains("<123456789@mail.example.com>") ||
                        message.Headers["In-Reply-To"].Contains("<123456789@mail.example.com>"))
                    {
                        EmailViewModel emailViewModel = new()
                        {
                            Subject = subject,
                            Body = textBody,
                        };
                        emailViewModels.Add(emailViewModel);
                    }
                }
                client.Disconnect(true);
            }
            return View(emailViewModels);
        }

        public IActionResult GetLastApiTime()
        {
            string apiDate = HttpContext.Session.GetString("ApiCallTime");
            if (apiDate != null)
            {
                DateTime datetime = DateTime.Parse(apiDate);
                DateTime currentDateTime = DateTime.Now;
                double difference = (currentDateTime - datetime).TotalMilliseconds;
                return Json(difference);
            }
            return Json(false);
        }

        public IActionResult AllPandingStudents()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            return View();
        }

        public IActionResult GetAllPendingStudents(SecondApiRequest secondApiRequest)
        {
            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllPendingStudents",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1" },
                token = secondApiRequest.token,


            };
            RoleBaseResponse<IList<Student>> roleBaseResponse = GetApiResponse<IList<Student>>(newSecondApiRequest);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);
        }


        public IActionResult ApproveRejectModal(int StudentId, bool ApproveOrReject, string Email)
        {
            SignUpViewModel signUpViewModel = new()
            {
                StudentId = StudentId,
                ApproveReject = ApproveOrReject,
                Email = Email
            };
            return View(signUpViewModel);
        }

        [HttpPost]
        public IActionResult ApproveRejectRequest(SignUpViewModel signUpViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";

            string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
            string Body = System.IO.File.ReadAllText(filePath);
            Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
            if (signUpViewModel.ApproveReject)
            {
                Body = Body.Replace("{{ Body }}", "Your Request is Approved By Hod You can Login Now");
                Body = Body.Replace("{{ Subject }}", "Approved Request");
            }
            else
            {
                Body = Body.Replace("{{ Body }}", "Sorry, Your Request is Rejected By Hod");
                Body = Body.Replace("{{ Subject }}", "Reject Request");
            }
            signUpViewModel.Body = Body;
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "ApproveRejectStudentRequest",
                DataObject = JsonConvert.SerializeObject(signUpViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);

            return RedirectToAction("AllPandingStudents");
        }

        [HttpGet]
        [Route("/Home/checkemail/{email}")]
        public IActionResult checkemail(string? email)
        {
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudentByEmail",
                DataObject = JsonConvert.SerializeObject(email),
            };
            RoleBaseResponse<StudentViewModel> roleBaseResponse = CallApiWithoutToken<StudentViewModel>(newSecondApiRequest);
            var ExistingEmail = roleBaseResponse.data.Email;
            return Json(new { exists = ExistingEmail });
        }

        [HttpGet]
        [Route("/Home/checkusername/{username}")]
        public IActionResult checkusername(string? username)
        {
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudentByUserName",
                DataObject = JsonConvert.SerializeObject(username),
            };
            RoleBaseResponse<StudentViewModel> roleBaseResponse = CallApiWithoutToken<StudentViewModel>(newSecondApiRequest);
            var ExistingEmail = roleBaseResponse.data.UserName;
            return Json(new { exists = ExistingEmail });
        }

        public IActionResult AllProfessors()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            return View();
        }

        [HttpPost]
        public IActionResult GetAllProfessors(SecondApiRequest secondApiRequest)
        {

            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
                JwtToken = secondApiRequest.token,
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetAllProfessors",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "AllProfessors",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1" },
                token = secondApiRequest.token,


            };
            RoleBaseResponse<IList<ProfessorHod>> roleBaseResponse = GetApiResponse<IList<ProfessorHod>>(newSecondApiRequest);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);
        }

        public IActionResult BlockUnblockProfessorModal(int Id, bool IsBlocked)
        {
            ProfessorHod professorHod = new()
            {
                Id = Id,
                IsBlocked = IsBlocked,

            };
            return View(professorHod);
        }

        [HttpPost]
        public IActionResult BlockUnblockProfessor(ProfessorHod professorHod)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "BlockUnblockProfessor",
                DataObject = JsonConvert.SerializeObject(professorHod),
                MethodType = "IsViewed",
                PageName = "AllProfessors",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
            if (professorHod.IsBlocked)
            {

                return RedirectToAction("AllProfessors");
            }
            else
            {
                return RedirectToAction("AllBlockedProfessors");

            }
        }

        public IActionResult BlockUnblockStudentModal(int Id, bool IsBlocked)
        {
            Student student = new()
            {
                StudentId = Id,
                IsBlocked = IsBlocked,

            };
            return View(student);
        }


        [HttpPost]
        public IActionResult BlockUnblockStudent(Student student)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "BlockUnblockStudent",
                DataObject = JsonConvert.SerializeObject(student),
                MethodType = "IsViewed",
                PageName = "AllProfessors",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
            return RedirectToAction("AdminIndex");
        }

        public IActionResult AllBlockedProfessors()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            return View();
        }

        [HttpPost]
        public IActionResult GetAllBlockedProfessors(SecondApiRequest secondApiRequest)
        {

            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
                JwtToken = secondApiRequest.token,
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetAllBlockedProfessors",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "AllProfessors",
                RoleId = secondApiRequest.RoleId,
                RoleIds = new List<string> { "1" },
                token = secondApiRequest.token,


            };
            RoleBaseResponse<IList<ProfessorHod>> roleBaseResponse = GetApiResponse<IList<ProfessorHod>>(newSecondApiRequest);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }
            return Json(roleBaseResponse);
        }

        public IActionResult ExchangeRates()
        {
            List<string> currencies = new()
            {
                "AUD", "AED", "CAD" , "CHF" ,"CNH", "EUR", "GBP","HKD", "INR", "JPY", "KWD", "KYD", "KZT", "NZD" ,"LAK", "USD",
            };
            ExchangeRate exchangeRate = new()
            {
                Currencies = currencies,
                StartDate = Convert.ToDateTime("2024-07-01"),
                EndDate = Convert.ToDateTime("2024-08-01"),
                BaseCurrency = "USD",
                ToCurrency = "GBP"
            };
            return View(exchangeRate);
        }

        public IActionResult GetExchangeRatesDetails(ExchangeRate exchangeRate)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetExchangeRates",
                DataObject = JsonConvert.SerializeObject(exchangeRate),
                MethodType = "IsViewed",
                PageName = "EmailLogs",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token

            };
            RoleBaseResponse<ExchangeRate> roleBaseResponse = GetApiResponse<ExchangeRate>(secondApiRequest);
            roleBaseResponse.data.ratesWithDate = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(roleBaseResponse.data.Rate);
            if (HttpContext.Session.GetInt32("UserId") == null || roleBaseResponse.IsAuthorize == false)
            {
                return Json(false);
            }

            string abbreviatedMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Convert.ToDateTime(exchangeRate.StartDate).Month);
            roleBaseResponse.MonthName = abbreviatedMonthName;
            return Json(roleBaseResponse);
        }

        public IActionResult AddRateAlert()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Currency",
                MethodName = "GetRateAlerts",
                DataObject = JsonConvert.SerializeObject(UserId),
                MethodType = "IsViewed",
                PageName = "CurrencyPair",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };
            RoleBaseResponse<IList<CurrencyRateViewModel>> roleBaseResponse = GetApiResponse<IList<CurrencyRateViewModel>>(newSecondApiRequest);
            return View(roleBaseResponse);
        }

        public IActionResult AddRateAlertModal()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            string email = HttpContext.Session.GetString("Email") ?? "";
            List<string> currencies = new()
            {
                "AUD", "AED", "CAD" , "EUR", "GBP","HKD", "INR", "JPY", "KWD", "NZD", "USD",
            };

            CurrencyRateViewModel currencyRateViewModel = new()
            {
                CurrencyPair = "USD" + "GBP",
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Currency",
                MethodName = "GetCurrencyPairRate",
                DataObject = JsonConvert.SerializeObject(currencyRateViewModel),
                MethodType = "IsViewed",
                PageName = "CurrencyPair",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };
            RoleBaseResponse<CurrencyRateViewModel> roleBaseResponse = GetApiResponse<CurrencyRateViewModel>(newSecondApiRequest);
            CurrencyRateViewModel currencyRateViewModel1 = roleBaseResponse.data;
            currencyRateViewModel1.BaseCurrency = currencyRateViewModel1.CurrencyPair.Substring(0, 3);
            currencyRateViewModel1.ToCurrency = currencyRateViewModel1.CurrencyPair.Substring(3);
            currencyRateViewModel1.Email = email;
            currencyRateViewModel1.Currencies = currencies;
            return View(currencyRateViewModel1);
        }


        public IActionResult UpdateRateAlertModal(int RateAlertId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            string email = HttpContext.Session.GetString("Email") ?? "";
            List<string> currencies = new()
            {
                "AUD", "AED", "CAD" , "EUR", "GBP","HKD", "INR", "JPY", "KWD", "NZD", "USD",
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Currency",
                MethodName = "GetRateAlertById",
                DataObject = JsonConvert.SerializeObject(RateAlertId),
                MethodType = "IsViewed",
                PageName = "CurrencyPair",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };
            RoleBaseResponse<CurrencyRateViewModel> roleBaseResponse = GetApiResponse<CurrencyRateViewModel>(newSecondApiRequest);
            CurrencyRateViewModel currencyRateViewModel1 = roleBaseResponse.data;
            currencyRateViewModel1.BaseCurrency = currencyRateViewModel1.CurrencyPair.Substring(0, 3);
            currencyRateViewModel1.ToCurrency = currencyRateViewModel1.CurrencyPair.Substring(3);
            currencyRateViewModel1.Email = email;
            currencyRateViewModel1.AskRate = currencyRateViewModel1.ExpectedRate;
            currencyRateViewModel1.Currencies = currencies;
            return View("AddRateAlertModal", currencyRateViewModel1);
        }

        public IActionResult GetPairCurrentRate(string currencyPair)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            CurrencyRateViewModel currencyRateViewModel = new()
            {
                CurrencyPair = currencyPair,
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Currency",
                MethodName = "GetCurrencyPairRate",
                DataObject = JsonConvert.SerializeObject(currencyRateViewModel),
                MethodType = "IsViewed",
                PageName = "CurrencyPair",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };
            RoleBaseResponse<CurrencyRateViewModel> roleBaseResponse = GetApiResponse<CurrencyRateViewModel>(newSecondApiRequest);
            CurrencyRateViewModel currencyRateViewModel1 = new();
            if (roleBaseResponse.data.Rate != 0)
            {
                currencyRateViewModel1 = roleBaseResponse.data;

                currencyRateViewModel1.BaseCurrency = currencyRateViewModel1.CurrencyPair.Substring(0, 3);
                currencyRateViewModel1.ToCurrency = currencyRateViewModel1.CurrencyPair.Substring(3);
            }
            return Json(currencyRateViewModel1);

        }

        public IActionResult UpsertRateAlert(CurrencyRateViewModel currencyRateViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (UserId != 0)
            {
                currencyRateViewModel.StudentId = UserId;
                currencyRateViewModel.CurrencyPair = currencyRateViewModel.BaseCurrency + currencyRateViewModel.ToCurrency;
                SecondApiRequest newSecondApiRequest = new()
                {
                    ControllerName = "Currency",
                    MethodName = "UpsertRateAlert",
                    DataObject = JsonConvert.SerializeObject(currencyRateViewModel),
                    MethodType = "IsViewed",
                    PageName = "CurrencyPair",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "3" },
                    token = token,
                };
                RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
            }
            return RedirectToAction("AddRateAlert");
        }

        public IActionResult RemoveRateAlertModal(int RateAlertId)
        {
            CurrencyRateViewModel currencyRateViewModel = new()
            {
                RateAlertId = RateAlertId,
            };
            return View(currencyRateViewModel);
        }

        [HttpPost]
        public IActionResult RemoveRateAlert(CurrencyRateViewModel currencyRateViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Currency",
                MethodName = "RemoveRateAlert",
                DataObject = JsonConvert.SerializeObject(currencyRateViewModel.RateAlertId),
                MethodType = "IsViewed",
                PageName = "CurrencyPair",
                RoleId = RoleId,
                RoleIds = new List<string> { "3" },
                token = token,
            };
            RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
            return RedirectToAction("AddRateAlert");
        }

        public IActionResult AllQueries()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            //if (RoleId != 1)
            //{
            //    return View("NotAuthorized");
            //}
            return View();
        }

        [HttpPost]
        public IActionResult GetAllQueries(SecondApiRequest secondApiRequest)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApiRequest.PageSize,
                StartIndex = secondApiRequest.StartIndex,
                OrderBy = secondApiRequest.OrderBy,
                OrderDirection = secondApiRequest.OrderDirection,
                searchQuery = secondApiRequest.searchQuery,
            };

            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetAllQueries",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,


            };
            RoleBaseResponse<IList<QueriesViewModel>> roleBaseResponse = GetApiResponse<IList<QueriesViewModel>>(newSecondApiRequest);
            return Json(roleBaseResponse);
        }

        public IActionResult AddQueryModal()
        {
            QueriesViewModel queriesViewModel = new();
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";

            string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
            string Body = System.IO.File.ReadAllText(filePath);
            Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
            queriesViewModel.Body = Body;

            SecondApiRequest secondApiRequest1 = new()
            {
                ControllerName = "Student",
                MethodName = "GetEmailsAndStudentIds",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,

            };
            RoleBaseResponse<IList<StudentsEmailAndIds>> roleBaseResponse1 = GetApiResponse<IList<StudentsEmailAndIds>>(secondApiRequest1);
            if (roleBaseResponse1.IsAuthorize == false)
            {
                return Json(false);
            }
            queriesViewModel.StudentsEmails = roleBaseResponse1.data;
            return View(queriesViewModel);
        }

        [HttpPost]
        public IActionResult AddQuery(QueriesViewModel queriesViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";

            int randomNumber = _random.Next(1000, 10000);
            queriesViewModel.TicketNumber = "#" + randomNumber.ToString();
            queriesViewModel.Subject = queriesViewModel.Subject + " " + queriesViewModel.TicketNumber;
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetEmailFromStudentId",
                DataObject = JsonConvert.SerializeObject(queriesViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<EmailViewModel> roleBaseResponse = GetApiResponse<EmailViewModel>(secondApiRequest);
            queriesViewModel.Email = roleBaseResponse.data.Email;
            if (queriesViewModel.Email != null)
            {
                SecondApiRequest newSecondApiRequest = new()
                {
                    ControllerName = "ProfessorHod",
                    MethodName = "AddQueries",
                    DataObject = JsonConvert.SerializeObject(queriesViewModel),
                    MethodType = "IsViewed",
                    PageName = "GetAllStudents",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token,
                };
                RoleBaseResponse<bool> newRoleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
            }
            return RedirectToAction("AllQueries");
        }

        public IActionResult QueryEmailDetails(int QueryId)
        {
            QueriesViewModel queriesViewModel = new()
            {
                QueryId = QueryId,
            };
            return View(queriesViewModel);
        }

        public IActionResult QueryAllReplies(int QueryId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetQueryDetail",
                DataObject = JsonConvert.SerializeObject(QueryId),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<QueriesViewModel> roleBaseResponse = GetApiResponse<QueriesViewModel>(newSecondApiRequest);
            QueriesViewModel queriesViewModel = roleBaseResponse.data;

            List<QueriesViewModel> queriesReply = new();
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]), SecureSocketOptions.SslOnConnect);
                client.Authenticate(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);

                var sentFolder = client.GetFolder(SpecialFolder.Sent);
                sentFolder.Open(MailKit.FolderAccess.ReadOnly);

                var query1 = SearchQuery.SubjectContains(queriesViewModel.Subject);

                var sentUids = sentFolder.Search(query1);
                foreach (var uid in sentUids)
                {
                    var message = sentFolder.GetMessage(uid);
                    string subject = message.Subject;
                    string textBody = message.TextBody;
                    string htmlBody = message.HtmlBody;
                    QueriesViewModel queriesViewModel1 = new()
                    {
                        Subject = subject,
                        Body = htmlBody,
                        Email = queriesViewModel.Email,
                        IsSentMe = true,
                        CreatedDate = DateTime.Parse(message.Date.Date.ToString("yyyy-MMM-dd"), CultureInfo.InvariantCulture),
                    };
                    queriesReply.Add(queriesViewModel1);
                }

                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadOnly);

                var query2 = SearchQuery.SubjectContains(queriesViewModel.Subject);

                var uids = inbox.Search(query2);
                foreach (var uid in uids)
                {
                    var message = inbox.GetMessage(uid);
                    string subject = message.Subject;

                    string textBody = message.TextBody;
                    string htmlBody = message.HtmlBody;
                    QueriesViewModel queriesViewModel1 = new()
                    {
                        Subject = subject,
                        Body = htmlBody,
                        Email = queriesViewModel.Email,
                        CreatedDate = DateTime.Parse(message.Date.ToString(), CultureInfo.InvariantCulture)
                    };
                    queriesReply.Add(queriesViewModel1);
                }



                client.Disconnect(true);
            }
            queriesViewModel.FirstName = queriesViewModel.FirstName.Substring(0, 1);
            queriesViewModel.LastName = queriesViewModel.LastName.Substring(0, 1);
            queriesReply.Sort((x, y) => DateTime.Compare(x.CreatedDate, y.CreatedDate));
            queriesViewModel.QueriesReply = queriesReply;
            //queriesViewModel.QueriesReply.OrderBy(q=> q.CreatedDate).ThenBy(q=> q.CreatedDate).ToList();
            return PartialView("QueryAllReplies", queriesViewModel);
        }


        public IActionResult AddReplyModal(int QueryId)
        {
            QueriesViewModel queriesViewModel = new();
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetQueryDetail",
                DataObject = JsonConvert.SerializeObject(QueryId),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" },
                token = token,
            };
            RoleBaseResponse<QueriesViewModel> roleBaseResponse = GetApiResponse<QueriesViewModel>(newSecondApiRequest);
            queriesViewModel = roleBaseResponse.data;
            string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
            string Body = System.IO.File.ReadAllText(filePath);
            Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
            queriesViewModel.Body = Body;
            return View(queriesViewModel);
        }

        [HttpPost]
        public IActionResult SendReplyEmail(QueriesViewModel queriesViewModel)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]), SecureSocketOptions.SslOnConnect);
                client.Authenticate(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                var inbox = client.Inbox;
                inbox.Open(MailKit.FolderAccess.ReadOnly);

                var query1 = SearchQuery.SubjectContains(queriesViewModel.Subject);

                var uids = inbox.Search(query1);
                var sentFolder = client.GetFolder(SpecialFolder.Sent);
                if (uids.Count == 0)
                {
                    sentFolder.Open(MailKit.FolderAccess.ReadOnly);
                    var newUids = sentFolder.Search(query1);
                    var message = sentFolder.GetMessage(newUids.First());
                    queriesViewModel.Subject = message.Subject;
                    queriesViewModel.MessageId = message.MessageId;
                }
                else
                {

                    var message = inbox.GetMessage(uids.First());
                    queriesViewModel.Subject = message.Subject;
                    queriesViewModel.MessageId = message.MessageId;
                }

                SecondApiRequest newSecondApiRequest = new()
                {
                    ControllerName = "ProfessorHod",
                    MethodName = "SendReplyEmail",
                    DataObject = JsonConvert.SerializeObject(queriesViewModel),
                    MethodType = "IsViewed",
                    PageName = "GetAllStudents",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1" },
                    token = token,
                };
                RoleBaseResponse<bool> roleBaseResponse = GetApiResponse<bool>(newSecondApiRequest);
                client.Disconnect(true);
            }
            return RedirectToAction("QueryEmailDetails", new { QueryId = queriesViewModel.QueryId });
        }

        public IActionResult Dashboard()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1 && RoleId != 2)
            {
                return View("NotAuthorized");
            }
            if (RoleId == 2)
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            return View();
        }

        public IActionResult GetDashboardRecordsCount()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 1;

            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetRecordsCount",
                DataObject = JsonConvert.SerializeObject(UserId),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<RecordsCountViewModel> roleBaseResponse = GetApiResponse<RecordsCountViewModel>(newSecondApiRequest);
            return Json(roleBaseResponse.data);
        }

        public IActionResult ExportAllCount()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 1;

            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "GetRecordsCount",
                DataObject = JsonConvert.SerializeObject(UserId),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<RecordsCountViewModel> roleBaseResponse = GetApiResponse<RecordsCountViewModel>(newSecondApiRequest);
            var todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            List<RecordsCountViewModel> list = new()
            {
                roleBaseResponse.data,
            };

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("School Management System");

                worksheet.Cells[1, 1, 1, 5].Merge = true;

                // Set title cell value and styles
                var titleCell = worksheet.Cells[1, 1];
                titleCell.Value = "School Management System";
                titleCell.Style.Font.Size = 16;
                titleCell.Style.Font.Color.SetColor(Color.White);
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleCell.Style.Fill.BackgroundColor.SetColor(Color.Blue);
                titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Adjust row height to fit title
                worksheet.Row(1).Height = 30; // Set to an appropriate value
                worksheet.Cells["A2"].Value = string.Empty;
                worksheet.Cells["A3"].Value = string.Empty;

                worksheet.Cells["A5"].Value = "Date";

                worksheet.Cells["A5"].Style.Font.Size = 16;
                var date = worksheet.Cells["B5"];
                date.Value = todayDate;
                date.Style.Font.Size = 16;
                date.Style.Font.Color.SetColor(System.Drawing.Color.MediumPurple);
                date.Style.Font.Bold = true;

                date.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["A7"].LoadFromCollection(list, true);

                var headerRow = worksheet.Cells[7, 1, 7, list[0].GetType().GetProperties().Length];

                headerRow.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRow.Style.Font.Bold = true;

                headerRow.Style.Font.Size = 12;
                headerRow.Style.Font.Color.SetColor(Color.White);
                headerRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRow.Style.Fill.BackgroundColor.SetColor(Color.Blue);
                headerRow.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells.AutoFitColumns();

                var result = package.GetAsByteArray();
                var fileName = "Counted_Records_" + todayDate + ".xlsx";

                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
        }


        public IActionResult ExportStudentList(DateOnly? FromDate, DateOnly? ToDate)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int UserId = HttpContext.Session.GetInt32("UserId") ?? 1;
            PaginationViewModel paginationViewModel = new()
            {
                FromDate = FromDate,
                ToDate = ToDate,
            };
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "ExportStudentList",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<IList<Student>> roleBaseResponse = GetApiResponse<IList<Student>>(newSecondApiRequest);
            var todayDate = DateTime.Now.ToString("yyyy-MM-dd");
            List<ExportStudentList> students = new();
            foreach (var data in roleBaseResponse.data)
            {
                ExportStudentList exportStudentList = _mapper.Map<ExportStudentList>(data);
                exportStudentList.CreatedDate = data.CreatedDate.ToString("yyyy-MMM-dd");
                DateTime birthdate = data.BirthDate ?? DateTime.Now;
                exportStudentList.BirthDate = birthdate.ToString("yyyy-MMM-dd");
                students.Add(exportStudentList);
            }
            var countList = new List<StudentsCountFromDateViewModel>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("School Management System");

                worksheet.Cells[1, 1, 1, 8].Merge = true;
                worksheet.View.ShowGridLines = false;
                // Set title cell value and styles
                var titleCell = worksheet.Cells[1, 1];
                titleCell.Value = "School Management System";
                titleCell.Style.Font.Size = 16;
                titleCell.Style.Font.Color.SetColor(Color.White);
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                titleCell.Style.Fill.BackgroundColor.SetColor(Color.Blue);
                titleCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                titleCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Adjust row height to fit title
                worksheet.Row(1).Height = 30; // Set to an appropriate value
                worksheet.Cells["A2"].Value = string.Empty;
                worksheet.Cells["A3"].Value = string.Empty;
                if (FromDate != null)
                {
                    worksheet.Cells["A5"].Value = "From";
                    worksheet.Cells["D5"].Value = "To";

                    worksheet.Cells["A5"].Style.Font.Size = 16;
                    worksheet.Cells["D5"].Style.Font.Size = 16;

                    var date = worksheet.Cells["B5"];
                    date.Value = FromDate.ToString();
                    date.Style.Font.Size = 16;
                    date.Style.Font.Color.SetColor(Color.MediumPurple);
                    date.Style.Font.Bold = true;
                    date.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var date2 = worksheet.Cells["E5"];
                    date2.Value = ToDate.ToString();
                    date2.Style.Font.Size = 16;
                    date2.Style.Font.Color.SetColor(Color.MediumPurple);
                    date2.Style.Font.Bold = true;
                    date2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    StudentsCountFromDateViewModel studentsCountFromDateViewModel = new()
                    {
                        FromDate = FromDate,
                        ToDate = ToDate,
                    };

                    SecondApiRequest newSecondApiRequest1 = new()
                    {
                        ControllerName = "Student",
                        MethodName = "GetStudentsCountFromDates",
                        DataObject = JsonConvert.SerializeObject(studentsCountFromDateViewModel),
                        MethodType = "IsViewed",
                        PageName = "GetAllStudents",
                        RoleId = RoleId,
                        RoleIds = new List<string> { "1", "2" },
                        token = token,
                    };

                    RoleBaseResponse<IList<StudentsCountFromDateViewModel>> roleBaseResponse1 = GetApiResponse<IList<StudentsCountFromDateViewModel>>(newSecondApiRequest1);
                    countList = roleBaseResponse1.data.ToList();
                }
                else
                {
                    worksheet.Cells["A5"].Value = "Date";

                    worksheet.Cells["A5"].Style.Font.Size = 16;
                    var date = worksheet.Cells["B5"];
                    date.Value = todayDate;
                    date.Style.Font.Size = 16;
                    date.Style.Font.Color.SetColor(System.Drawing.Color.MediumPurple);
                    date.Style.Font.Bold = true;

                    date.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }


                worksheet.Cells[7, 1, 7, 8].Merge = true;

                var headerCell = worksheet.Cells[7, 1];
                headerCell.Value = "FromDate to ToDate All Students";
                headerCell.Style.Font.Size = 16;
                headerCell.Style.Font.Color.SetColor(Color.White);
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCell.Style.Fill.BackgroundColor.SetColor(Color.Blue);
                headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["A8"].LoadFromCollection(students, true);

                if (FromDate != null && countList.Count > 0)
                {
                    var worksheet2 = package.Workbook.Worksheets.Add("Sheet2");
                    worksheet2.Hidden = OfficeOpenXml.eWorkSheetHidden.Hidden;
                    int row = worksheet.Dimension.End.Row + 2;

                    worksheet2.Cells[row, 1].Value = "Date";
                    worksheet2.Cells[row, 2].Value = "Student Count";

                    row++;

                    foreach (var item in countList)
                    {
                        worksheet2.Cells[row, 1].Value = item.CreatedDate?.ToString("yyyy-MM-dd") ?? "N/A";
                        worksheet2.Cells[row, 2].Value = item.StudentsCount;
                        row++;
                    }


                    var chart = (ExcelBarChart)worksheet.Drawings.AddChart("StaticChart", eChartType.ColumnClustered3D);
                    chart.Title.Text = "Student Count ";
                    chart.SetPosition(6, 0, 9, 0);
                    int numberOfRecords = countList.Count;
                    int baseWidth = 600;
                    int baseHeight = 400;
                    int widthPerRecord = 30;
                    int heightPerRecord = 20;

                    int chartWidth = baseWidth + (widthPerRecord * numberOfRecords);
                    int chartHeight = baseHeight + (heightPerRecord * numberOfRecords);

                    int minWidth = 600;
                    int minHeight = 400;
                    int maxWidth = 1200;
                    int maxHeight = 800;

                    chartWidth = Math.Max(minWidth, Math.Min(chartWidth, maxWidth));
                    chartHeight = Math.Max(minHeight, Math.Min(chartHeight, maxHeight));
                    chart.SetSize(chartWidth, chartHeight);
                    var dataRange = worksheet2.Cells[$"B{row - countList.Count}:B{row - 1}"];
                    var categoryRange = worksheet2.Cells[$"A{row - countList.Count}:A{row - 1}"];

                    var series = chart.Series.Add(dataRange, categoryRange);
                    chart.StyleManager.SetChartStyle(ePresetChartStyle.Bar3dChartStyle9, ePresetChartColors.ColorfulPalette3);
                    chart.DataLabel.ShowCategory = false;
                    chart.DataLabel.Font.Bold = true;
                    chart.DataLabel.Font.Color = Color.White;
                    chart.DataLabel.ShowValue = true;
                    worksheet.Calculate();
                }


                var headerRow = worksheet.Cells[8, 1, 8, worksheet.Dimension.End.Column];

                headerRow.Style.Font.Bold = true;
                worksheet.Cells.AutoFitColumns();

                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                for (int row = 1; row <= rowCount; row++)
                {
                    for (int col = 1; col <= colCount; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Text))
                        {
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }
                    }
                }

                var result = package.GetAsByteArray();
                var fileName = "Students_List" + todayDate + ".xlsx";

                return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }


        }

        public IActionResult DownloadPDF(DateOnly? FromDate, DateOnly? ToDate)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            PaginationViewModel paginationViewModel = new()
            {
                FromDate = FromDate,
                ToDate = ToDate,
            };
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            SecondApiRequest newSecondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "ExportStudentList",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };
            RoleBaseResponse<IList<Student>> roleBaseResponse = GetApiResponse<IList<Student>>(newSecondApiRequest);
            var todayDate = DateTime.Now.ToString("dd MMM yyyy");
            List<ExportStudentList> students = new();
            foreach (var data in roleBaseResponse.data)
            {
                ExportStudentList exportStudentList = _mapper.Map<ExportStudentList>(data);
                exportStudentList.CreatedDate = data.CreatedDate.ToString("yyyy-MMM-dd");
                DateTime birthdate = data.BirthDate ?? DateTime.Now;
                exportStudentList.BirthDate = birthdate.ToString("yyyy-MMM-dd");
                students.Add(exportStudentList);
            }

            string filePath = Path.Combine("wwwroot", "EmailTemplate", "PdfTemplate.html");
            string template = System.IO.File.ReadAllText(filePath);

            string tableRows = string.Join("\n", students.Select(d => $@"
            <tr>
                <td>{d.CreatedDate}</td>
                <td>{d.FirstName}</td>
                <td>{d.LastName}</td>
                <td>{d.BirthDate}</td>
                <td>{d.CourseName}</td>
                <td>{d.UserName}</td>
                <td>{d.Email}</td>
                <td>{d.StudentId}</td>
            </tr>
<tr>
                <td>{d.CreatedDate}</td>
                <td>{d.FirstName}</td>
                <td>{d.LastName}</td>
                <td>{d.BirthDate}</td>
                <td>{d.CourseName}</td>
                <td>{d.UserName}</td>
                <td>{d.Email}</td>
                <td>{d.StudentId}</td>
            </tr>"));

            string htmlContent = template.Replace("{{tableRows}}", tableRows);
            //if (FromDate != null)
            //{
            //    htmlContent = htmlContent.Replace("{{FromDate}}", FromDate.ToString());
            //    htmlContent = htmlContent.Replace("{{ToDate}}", ToDate.ToString());
            //}
            //else
            //{
            //    htmlContent = htmlContent.Replace("{{FromDate}}", "");

            //    htmlContent = htmlContent.Replace("{{ToDate}}", todayDate.ToString());
            //}
            var converter = new HtmlToPdf();
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 10;
            converter.Options.MarginBottom = 0;
            converter.Options.DisplayHeader = true;
            converter.Options.DisplayFooter = true;
            converter.Header.DisplayOnFirstPage = true;
            converter.Header.DisplayOnOddPages = true;
            converter.Header.DisplayOnEvenPages = true;
            converter.Header.Height = 230;
            converter.Footer.Height = 100;  

            string headerHtmlPath = Path.Combine("wwwroot", "EmailTemplate", "PdfHeader.html");
            string headerHtmlContent = System.IO.File.ReadAllText(headerHtmlPath);

            string updatedHeaderHtmlContent = headerHtmlContent
                .Replace("{{FromDate}}", FromDate != null ? FromDate.Value.ToString("dd MMM") : "")
                .Replace("{{ToDate}}", FromDate != null ? ToDate?.ToString("dd MMM yyyy") : todayDate)
                 .Replace("{{HodName}}", "Kishan Dabhoya")
                .Replace("{{Email}}", "dabhoyakishan12@gmail.com")
                .Replace("{{todayDate}}", DateTime.Now.ToString("dd MMM, yyyy"));

            PdfHtmlSection headerHtmlSection = new PdfHtmlSection(updatedHeaderHtmlContent,string.Empty)
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };

            converter.Header.Add(headerHtmlSection);
            PdfImageSection imageSection = new PdfImageSection(30, 23, 35,"wwwroot/EmailTemplate/school_logo.png");
            imageSection.Width = 35;
            imageSection.Height = 35;

            // Add the image to the header
            converter.Header.Add(imageSection);

            string footerHtmlPath = Path.Combine("wwwroot", "EmailTemplate", "PdfFooter.html");
            string footerHtmlContent = System.IO.File.ReadAllText(footerHtmlPath);

            PdfHtmlSection footerHtmlSection = new PdfHtmlSection(footerHtmlContent, string.Empty)
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };
            PdfTextSection text = new PdfTextSection(-35, 20, "Page: {page_number} ", new System.Drawing.Font("Arial", 12));
            text.HorizontalAlign = PdfTextHorizontalAlign.Right;
            converter.Footer.Add(text);
            converter.Footer.Add(footerHtmlSection); 

            var document = converter.ConvertHtmlString(htmlContent);
            if (document.Pages.Count > 1)
            {
                // Remove the last page if it is blank
                PdfPage lastPage = document.Pages[document.Pages.Count - 1];
                    document.Pages.Remove(lastPage);
            }
            using var stream = new MemoryStream();
            document.Save(stream);

            var fileName = "Students_List" + todayDate + ".pdf";

            return File(stream.ToArray(), "application/pdf", fileName);
        }
    }
}