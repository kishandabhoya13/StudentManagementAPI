using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services;
using StudentManagment.Services.Interface;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Razor.Tokenizer;

namespace StudentManagment.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseServices _baseServices;
        private readonly IMapper _mapper;
        public HomeController(ILogger<HomeController> logger, IBaseServices baseServices, IMapper mapper)
        {
            _logger = logger;
            _baseServices = baseServices;
            _mapper = mapper;
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
            RoleBaseResponse<Student> roleBaseResponse = new()
            {
                Courses = _baseServices.GetAllCourses(token, RoleId)
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
            RoleBaseResponse<Student> roleBaseResponse = _baseServices.GetAllStudentsWithPagination(secondApiRequest);
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("ProfessorHodLogin", "Login");
            }
            if (roleBaseResponse.Role == "2")
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (roleBaseResponse.Role == "1")
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            return Json(roleBaseResponse);
        }


        [HttpGet]
        public IActionResult GetStudentProfessorChartDetails(int month, int year)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            RoleBaseResponse<CountStudentProfessor> roleBaseResponse = _baseServices.GetDayWiseProfStudentCount(month, year, RoleId, token);
            string abbreviatedMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            roleBaseResponse.MonthName = abbreviatedMonthName;
            roleBaseResponse.year = year;
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime currentDate = startDate;
            while (currentDate <= endDate)
            {
                CountStudentProfessor countEmailViewModel = new()
                {
                    CreatedDate1 = currentDate,
                    CreatedDate2 = currentDate,
                    ProfessorDayWiseCount= 0,
                    StudentDayWiseCount = 0,
                };
                ((IList<CountStudentProfessor>)roleBaseResponse.data).Add(countEmailViewModel);
                currentDate = currentDate.AddDays(1);
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
            if ((StudentId == null && RoleId == 3) || (StudentId == null && RoleId == 2))
            {
                return View("NotAuthorized");
            }
            if (StudentId != null && StudentId != 0)
            {
                string token = HttpContext.Session.GetString("Jwt");
                SecondApiRequest secondApiRequest = new()
                {
                    RoleId = RoleId,
                    MethodType = "IsManaged",
                    PageName = "EditStudent"
                };
                Student student = _baseServices.GetStudentByMaster(StudentId ?? 0, token, secondApiRequest);
                studentViewModel = _mapper.Map<StudentViewModel>(student);
            }
            studentViewModel.Courses = _baseServices.GetAllCourses(HttpContext.Session.GetString("Jwt") ?? "", RoleId);
            return View(studentViewModel);
        }

        [HttpPost]
        public IActionResult UpsertStudent(StudentViewModel studentViewModel)
        {
            studentViewModel.JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            studentViewModel.RoleId = HttpContext.Session.GetInt32("RoleId");
            bool isSuccess = _baseServices.UpsertStudent(studentViewModel);
            if (isSuccess)
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
            course.JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            bool isSuccess = _baseServices.InsertCouse(course, RoleId);
            if (isSuccess)
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

            RoleBaseResponse<Book> roleBaseResponse = new()
            {
                Courses = _baseServices.GetAllCourses(token, RoleId),
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
            RoleBaseResponse<Book> roleBaseResponse = _baseServices.GetAllBooksWithPagination(secondApiRequest);
            //foreach(Book book in roleBaseResponse.data)
            //{
            //    if(book.Photo != null)
            //    {
            //        byte[] bytedata = book.Photo;
            //        string data = Convert.ToBase64String(bytedata);
            //        book.Photos = string.Format("data:image/png;base64,{0}", data);
            //    }

            //}
            return Json(roleBaseResponse);

        }


        public IActionResult AddEditBook(int? BookId)
        {
            BookViewModel bookViewModel = new();
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            if (BookId != 0)
            {
                string token = HttpContext.Session.GetString("Jwt");
                SecondApiRequest secondApiRequest = new()
                {
                    RoleId = RoleId,
                    MethodType = "IsViewed",
                    PageName = "GetAllBooks"
                };
                Book book = _baseServices.GetBook(BookId ?? 0, token, secondApiRequest);
                bookViewModel = _mapper.Map<BookViewModel>(book);
                if (book.Photo != null)
                {
                    using var stream = new MemoryStream(book.Photo);

                    IFormFile file = new FormFile(stream, 0, book.Photo.Length, "name", "fileName");
                    bookViewModel.PhotoFile = file;
                }

            }

            bookViewModel.Courses = _baseServices.GetAllCourses(HttpContext.Session.GetString("Jwt") ?? "", RoleId);
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

        public IActionResult SendEmailModal(int ScheduledEmailId)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId != 1)
            {
                return View("NotAuthorized");
            }
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            EmailViewModel emailViewModel = new();
            if (ScheduledEmailId != 0)
            {
                emailViewModel = _baseServices.GetScheduledEmailById(RoleId, token, ScheduledEmailId);
            }
            emailViewModel.StudentsEmails = _baseServices.GetEmailsAndIds(RoleId, token);
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
        public IActionResult GetChartDetails(int month,int year)
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            RoleBaseResponse<CountEmailViewModel> roleBaseResponse = _baseServices.GetDayWiseEmailCount(month,year, RoleId, token);
            string abbreviatedMonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
            roleBaseResponse.MonthName = abbreviatedMonthName;
            roleBaseResponse.year = year;
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            DateTime currentDate = startDate;
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
            return Json(roleBaseResponse);
        }
    }
}