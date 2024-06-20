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
using System.Data;
using System.Diagnostics;

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

            //if(StudentId == 0)
            //{
            //    return RedirectToAction("Login", "Login");
            //}
            string token = HttpContext.Session.GetString("Jwt");
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            SecondApiRequest secondApiRequest = new()
            {
                RoleId = RoleId,
                MethodType = "IsViewed",
                PageName = "EditStudent"
            };
            Student student = _baseServices.GetStudentByMaster(StudentId, token, secondApiRequest);
            if (student.StudentId == 0)
            {
                return RedirectToAction("Login", "Login");
            }
            StudentViewModel studentViewModel = _mapper.Map<StudentViewModel>(student);
            return View(studentViewModel);
        }

        public IActionResult AdminIndex()
        {
            string token = HttpContext.Session.GetString("Jwt") ?? "";
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (RoleId == 2)
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            else if (RoleId == 0)
            {
                return View("NotAuthorized");
            }
            RoleBaseResponse<Student> roleBaseResponse = new()
            {
                OrderBys = new List<OrderByViewModel>()
                {
                    new OrderByViewModel{OrderByName = "StudentId", OrderByValues="StudentId"},
                    new OrderByViewModel{OrderByName = "First Name", OrderByValues="FirstName"},
                    new OrderByViewModel{OrderByName = "Last Name", OrderByValues="LastName"},
                    new OrderByViewModel{OrderByName = "BirthDate", OrderByValues="BirthDate"},
                    new OrderByViewModel{OrderByName = "UserName", OrderByValues="UserName"},

                }
            };
            roleBaseResponse.Courses = _baseServices.GetAllCourses(token, RoleId);

            SecondApiRequest secondApiRequest = new()
            {
                token = token,
                RoleId = RoleId,
            };
            //roleBaseResponse = _baseServices.GetAllStudentsWithPagination(secondApiRequest);
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
            else if (roleBaseResponse.Role == null)
            {
                return View("NotAuthorized");
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
            if (RoleId == 2)
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (RoleId == 1)
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            else if (RoleId == 0)
            {
                return View("NotAuthorized");
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
            secondApiRequest.RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            secondApiRequest.token = HttpContext.Session.GetString("Jwt") ?? "";
            RoleBaseResponse<Book> roleBaseResponse = _baseServices.GetAllBooksWithPagination(secondApiRequest);
            return Json(roleBaseResponse);

        }

        public IActionResult AddBook()
        {
            int RoleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            BookViewModel bookViewModel = new()
            {
                Courses = _baseServices.GetAllCourses(HttpContext.Session.GetString("Jwt") ?? "", RoleId)
            };
            return View(bookViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertBook(BookViewModel bookViewModel)
        {
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
    }
}