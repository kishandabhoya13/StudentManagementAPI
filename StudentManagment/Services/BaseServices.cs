using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
using StudentManagement.Models.DTO;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagment.Services
{
    public class BaseServices : IBaseServices
    {
        Uri BaseAddress = new Uri("https://localhost:7105/StudentApi");
        Uri SecondAddress = new("https://localhost:7105/MasterApi");
        private readonly HttpClient _httpClient;
        public BaseServices()
        {
            _httpClient = new HttpClient();
            //_httpClient.BaseAddress = BaseAddress;
            _httpClient.BaseAddress = SecondAddress;
        }

        public Student CheckLoginDetails(StudentViewModel studentViewModel)
        {
            string url = _httpClient.BaseAddress + "/Student/LoginStudentDetails/Login";
            Student student = new();
            StudentLoginViewModel studentLoginViewModel = new()
            {
                UserName = studentViewModel.UserName,
                Password = studentViewModel.Password,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "LoginStudentDetails",
                DataObject = JsonConvert.SerializeObject(studentLoginViewModel),
            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                APIResponse<Student> apiResponse = JsonConvert.DeserializeObject<APIResponse<Student>>(data);
                if (apiResponse.IsSuccess && apiResponse.result != null)
                {
                    student = apiResponse.result;
                    // Now you can work with the student object
                }
            }
            return student;
        }

        public ProfessorHod CheckAdminLoginDetails(AdminViewModel adminViewModel)
        {
            string url = _httpClient.BaseAddress + "/ProfessorHod/LoginDetails/Login";
            ProfessorHod professorHod = new();
            StudentLoginViewModel studentLoginViewModel = new()
            {
                UserName = adminViewModel.UserName,
                Password = adminViewModel.Password,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "LoginDetails",
                DataObject = JsonConvert.SerializeObject(studentLoginViewModel),
            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                APIResponse<ProfessorHod> apiResponse = JsonConvert.DeserializeObject<APIResponse<ProfessorHod>>(data);
                if (apiResponse.IsSuccess && apiResponse.result != null)
                {
                    professorHod = apiResponse.result;
                    // Now you can work with the student object
                }
            }
            return professorHod;
        }

        public RoleBaseResponse GetAllStudents(string token,int RoleId)
        {
            string url = _httpClient.BaseAddress + "/Student/GetAllStudents/";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllStudents",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "AdminIndex",
                RoleId = RoleId,
            };
            _httpClient.DefaultRequestHeaders.Add("token", token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
            }
            return roleBaseResponse;
        }

        public RoleBaseResponse GetAllStudentsWithPagination(SecondApiRequest secondApi)
        {
            string url = _httpClient.BaseAddress + "/Student/GetAllStudents/";
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApi.PageSize,
                StartIndex = secondApi.StartIndex,
                OrderBy = secondApi.OrderBy,
                OrderDirection = secondApi.OrderDirection,
                searchQuery = secondApi.searchQuery,
                JwtToken = secondApi.token
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllStudents",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "AdminIndex",
                RoleId = secondApi.RoleId,
            };
            _httpClient.DefaultRequestHeaders.Add("token", secondApi.token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
            }
            return roleBaseResponse;
        }

        public Student GetStudentByMaster(int id, string token,SecondApiRequest apiRequest)
        {
            string url = _httpClient.BaseAddress + "/Student/GetStudent/";
            Student student = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudent",
                DataObject = JsonConvert.SerializeObject(id.ToString()),
                MethodType = apiRequest.MethodType,
                PageName = apiRequest.PageName,
                RoleId = apiRequest.RoleId,
            };
            _httpClient.DefaultRequestHeaders.Add("token", token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Student> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Student>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    student = aPIResponse.result;
                }
            }
            return student;
        }

        public List<Course> GetAllCourses(string token,int RoleId)
        {
            string url = _httpClient.BaseAddress + "/Course/GetAllCourses/";
            List<Course> courses = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetAllCourses",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllCourses",
                RoleId = RoleId,
            };
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", token);

            }
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<List<Course>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<List<Course>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    courses = aPIResponse.result;
                }
            }
            return courses;
        }

        public Course GetCourseDetailById(int id,int RoleId)
        {
            string url = _httpClient.BaseAddress + "/Course/GetCourse/";
            Course course = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetCourse",
                DataObject = JsonConvert.SerializeObject(id.ToString()),
                MethodType= "IsViewed",
                PageName = "GetCourse",
                RoleId = RoleId,
            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Course> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Course>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    course = aPIResponse.result;
                }
            }
            return course;
        }

        public bool UpdateJwtToken(string token, int StudentId, string currentToken)
        {
            UpdateJwtViewModel updateJwtViewModel = new()
            {
                Id = StudentId,
                token = token,
            };
            _httpClient.DefaultRequestHeaders.Add("token", currentToken);
            string url = _httpClient.BaseAddress + "/Student/UpdateStudentJwtToken";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "UpdateStudentJwtToken",
                DataObject = JsonConvert.SerializeObject(updateJwtViewModel),
            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public bool UpdateProfessorHodJwtToken(string token, int Id, string currentToken)
        {
            UpdateJwtViewModel updateJwtViewModel = new()
            {
                Id = Id,
                token = token,
            };
            _httpClient.DefaultRequestHeaders.Add("token", currentToken);
            string url = _httpClient.BaseAddress + "/ProfessorHod/UpdateProfessorHodJwtToken";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "UpdateProfessorHodJwtToken",
                DataObject = JsonConvert.SerializeObject(updateJwtViewModel),
            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public bool UpsertStudent(StudentViewModel studentViewModel)
        {
            _httpClient.DefaultRequestHeaders.Add("token", studentViewModel.JwtToken);
            string url = "";
            SecondApiRequest secondApiRequest = new();
            if (studentViewModel.StudentId != 0)
            {
                url = _httpClient.BaseAddress + "/Student/UpdateStudent";
                secondApiRequest.ControllerName = "Student";
                secondApiRequest.MethodName = "UpdateStudent";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(studentViewModel);
                secondApiRequest.PageName = "EditStudent";
                secondApiRequest.MethodType = "IsManaged";
                secondApiRequest.RoleId = studentViewModel.RoleId;
            }
            else
            {
                url = _httpClient.BaseAddress + "/Student/CreateStudent";
                secondApiRequest.ControllerName = "Student";
                secondApiRequest.MethodName = "CreateStudent";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(studentViewModel);
                secondApiRequest.PageName = "CreateStudent";
                secondApiRequest.MethodType = "IsInsert";
                secondApiRequest.RoleId = studentViewModel.RoleId;
            }
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Course> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Course>>(data);
                if (aPIResponse.IsSuccess)
                {
                    return true;
                }
            }
            return false;
        }

        public bool InsertCouse(Course course,int RoleId)
        {
            _httpClient.DefaultRequestHeaders.Add("token", course.JwtToken);
            SecondApiRequest secondApiRequest = new();
            string url = _httpClient.BaseAddress + "/Course/CreateCourse";
            secondApiRequest.ControllerName = "Course";
            secondApiRequest.MethodName = "CreateCourse";
            secondApiRequest.DataObject = JsonConvert.SerializeObject(course);
            secondApiRequest.PageName = "CreateCourse";
            secondApiRequest.MethodType = "IsInsert";
            secondApiRequest.RoleId = RoleId;
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Course> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Course>>(data);
                if (aPIResponse.IsSuccess)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
