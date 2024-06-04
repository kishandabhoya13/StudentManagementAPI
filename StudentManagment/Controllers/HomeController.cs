using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
using System.Diagnostics;

namespace StudentManagment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBaseServices _baseServices;
        private readonly IMapper _mapper;
        public HomeController(ILogger<HomeController> logger,IBaseServices baseServices,IMapper mapper)
        {
            _logger = logger;
            _baseServices = baseServices;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            int StudentId = HttpContext.Session.GetInt32("StudentId") ?? 0;

            //if(StudentId == 0)
            //{
            //    return RedirectToAction("Login", "Login");
            //}
            string token = HttpContext.Session.GetString("Jwt");
            Student student = _baseServices.GetStudentByMaster(StudentId,token);
            if(student.StudentId == 0)
            {
                return RedirectToAction("Login", "Login");
            }
            StudentViewModel studentViewModel = _mapper.Map<StudentViewModel>(student); 
            Course course = _baseServices.GetCourseDetailById(student.CourseId);
            studentViewModel.CourseName = course.Name;
            return View(studentViewModel);
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
    }
}