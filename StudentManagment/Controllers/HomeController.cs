using AutoMapper;
using AutoMapper.Internal;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Razor.Tokenizer;

namespace StudentManagment.Controllers
{

    public class HomeController : MasterController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseServices _baseServices;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ICacheServices _cacheServices;
        public HomeController(ILogger<HomeController> logger, IBaseServices baseServices,
            IMapper mapper, IConfiguration configuration, ICacheServices cacheServices)
        {
            _logger = logger;
            _baseServices = baseServices;
            _mapper = mapper;
            _cacheServices = cacheServices;
            _configuration = configuration;
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
                RoleId = RoleId,
                MethodType = "IsViewed",
                PageName = "EditStudent"
            };
            Student student = _baseServices.GetStudentByMaster(StudentId, token, secondApiRequest);
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
                return View("NotAuthorized");
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
            RoleBaseResponse<IList<Student>> roleBaseResponse = GetApiResponse<IList<Student>>(newSecondApiRequest);
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
                RoleIds = new List<string> { "1" },
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
                RoleIds = new List<string> { "1", "2" },
                token = token,
            };

            roleBaseResponse = GetApiResponse<IList<Course>>(CourseecondApiRequest);
            studentViewModel.Courses = roleBaseResponse.data;
            return View(studentViewModel);
        }

        [HttpPost]
        public IActionResult UpsertStudent(StudentViewModel studentViewModel)
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            studentViewModel.RoleId = HttpContext.Session.GetInt32("RoleId");
            SecondApiRequest secondApiRequest = new();
            if (studentViewModel.StudentId != 0)
            {
                secondApiRequest.ControllerName = "Student";
                secondApiRequest.MethodName = "UpdateStudent";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(studentViewModel);
                secondApiRequest.PageName = "EditStudent";
                secondApiRequest.MethodType = "IsManaged";
                secondApiRequest.RoleId = studentViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1" };
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
            bool isSuccess = await _baseServices.UpsertBook(bookViewModel);
            if (isSuccess)
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
            bool isSuccess = _baseServices.DeleteBook(bookViewModel);
            if (isSuccess)
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
                    JwtToken = HttpContext.Session.GetString("Jwt") ?? "",
                    RoleId = HttpContext.Session.GetInt32("RoleId"),
                };
                Book book = _baseServices.GetBookPhoto(bookViewModel);
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
            EmailViewModel emailViewModel = new();
            emailViewModel.StudentId = 0;
            if (ScheduledEmailId != 0)
            {
                emailViewModel = _baseServices.GetScheduledEmailById(RoleId, token, ScheduledEmailId);
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
            emailViewModel.StudentsEmails = _baseServices.GetEmailsAndIds(RoleId, token);
            string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
            string Body = System.IO.File.ReadAllText(filePath);
            Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
            emailViewModel.Body = Body;
            return View(emailViewModel);
        }

        [HttpPost]
        public IActionResult SendEmail(EmailViewModel emailViewModel)
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
                _baseServices.UpdateScheduledEmailLog(emailViewModel);
            }
            else
            {

                emailViewModel.SentBy = HttpContext.Session.GetInt32("UserId") ?? 0;
                emailViewModel.Emails = new();
                if (emailViewModel.StudentId == 0)
                {
                    EmailViewModel newemailViewModel = new();
                    newemailViewModel.StudentsEmails = _baseServices.GetEmailsAndIds(RoleId, token);
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
                    emailViewModel.Email = _baseServices.GetEmailFromStudentId(emailViewModel);
                    emailViewModel.Emails.Add(emailViewModel.Email);
                }
                //string filePath = Path.Combine("wwwroot", "EmailTemplate", "EmailTemplate.html");
                //string Body = System.IO.File.ReadAllText(filePath);
                //Body = Body.Replace("{{ Body }}", emailViewModel.Body);
                //Body = Body.Replace("{{ Subject }}", emailViewModel.Subject);
                //Body = Body.Replace("{{ date }}", DateTime.Now.ToString());
                //emailViewModel.Body = Body;
                _baseServices.SendEmail(emailViewModel);
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
            RoleBaseResponse<ScheduledEmailViewModel> roleBaseResponse = _baseServices.GetAllScheduledEmail(secondApiRequest);
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
            RoleBaseResponse<CountEmailViewModel> roleBaseResponse = _baseServices.GetDayWiseEmailCount(month, year, RoleId, token);

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
    }
}