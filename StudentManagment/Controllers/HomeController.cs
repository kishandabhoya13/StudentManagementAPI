using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services;
using StudentManagment.Services.Interface;
using System.Diagnostics;

namespace StudentManagment.Controllers
{

    [ServiceFilter(typeof(CustomExceptionFilter))]
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
            Student student = _baseServices.GetStudentByMaster(StudentId, token);
            if (student.StudentId == 0)
            {
                return RedirectToAction("Login", "Login");
            }
            StudentViewModel studentViewModel = _mapper.Map<StudentViewModel>(student);
            Course course = _baseServices.GetCourseDetailById(student.CourseId);
            studentViewModel.CourseName = course.Name;
            return View(studentViewModel);
        }

        public IActionResult AdminIndex()
        {
            string token = HttpContext.Session.GetString("Jwt");
            RoleBaseResponse roleBaseResponse = _baseServices.GetAllStudents(token);
            if (roleBaseResponse.Role == "2")
            {
                HttpContext.Session.SetString("Role", "Professor");
            }
            else if (roleBaseResponse.Role == "1")
            {
                HttpContext.Session.SetString("Role", "Hod");
            }
            return View(roleBaseResponse);
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
            if (StudentId != null && StudentId != 0)
            {
                string token = HttpContext.Session.GetString("Jwt");
                Student student = _baseServices.GetStudentByMaster(StudentId ?? 0, token);
                studentViewModel = _mapper.Map<StudentViewModel>(student);
            }
            studentViewModel.Courses = _baseServices.GetAllCourses(HttpContext.Session.GetString("Jwt") ?? "");
            return View(studentViewModel);
        }

        [HttpPost]
        public IActionResult UpsertStudent(StudentViewModel studentViewModel)
        {
            studentViewModel.JwtToken = HttpContext.Session.GetString("Jwt") ?? "";
            bool isSuccess = _baseServices.UpsertStudent(studentViewModel);
            if (isSuccess)
            {
                if(HttpContext.Session.GetString("Role") != null)
                {
                    return RedirectToAction("AdminIndex", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(studentViewModel);
            }
        }
    }
}