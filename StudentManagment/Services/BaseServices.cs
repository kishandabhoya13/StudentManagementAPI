using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentManagement.Models;
using StudentManagement.Models.DTO;
using StudentManagement_API.DataContext;
using StudentManagement_API.Services.CacheService;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentManagment.Services
{
    public class BaseServices : IBaseServices
    {
        Uri BaseAddress = new Uri("https://localhost:7105/StudentApi");
        Uri SecondAddress = new("https://localhost:7105/MasterApi");
        string apiVersionUrl = "https://localhost:7105/api/Login/GetApiVersion";

        private readonly HttpClient _httpClient;
        private readonly ICacheServices _cacheServices;
        public BaseServices(ICacheServices cacheServices)
        {
            _httpClient = new HttpClient();
            //_httpClient.BaseAddress = BaseAddress;
            _httpClient.BaseAddress = SecondAddress;
            _cacheServices = cacheServices;

        }

        //public Student CheckLoginDetails(StudentViewModel studentViewModel)
        //{
        //    string url = _httpClient.BaseAddress + "/Student/LoginStudentDetails/Login";
        //    Student student = new();
        //    StudentLoginViewModel studentLoginViewModel = new()
        //    {
        //        UserName = studentViewModel.UserName,
        //        Password = studentViewModel.Password,
        //    };
        //    SecondApiRequest secondApiRequest = new()
        //    {
        //        ControllerName = "Student",
        //        MethodName = "LoginStudentDetails",
        //        DataObject = JsonConvert.SerializeObject(studentLoginViewModel),
        //    };
        //    var serializedData = JsonConvert.SerializeObject(secondApiRequest);
        //    var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string data = response.Content.ReadAsStringAsync().Result;
        //        APIResponse<Student> apiResponse = JsonConvert.DeserializeObject<APIResponse<Student>>(data);
        //        if (apiResponse.IsSuccess && apiResponse.result != null)
        //        {
        //            student = apiResponse.result;
        //            // Now you can work with the student object
        //        }
        //    }
        //    return student;
        //}

        //public ProfessorHod CheckAdminLoginDetails(AdminStudentViewModel adminViewModel)
        //{
        //    string url = _httpClient.BaseAddress + "/ProfessorHod/LoginDetails/Login";
        //    ProfessorHod professorHod = new();
        //    StudentLoginViewModel studentLoginViewModel = new()
        //    {
        //        UserName = adminViewModel.UserName,
        //        Password = adminViewModel.Password,
        //    };
        //    SecondApiRequest secondApiRequest = new()
        //    {
        //        ControllerName = "ProfessorHod",
        //        MethodName = "LoginDetails",
        //        DataObject = JsonConvert.SerializeObject(studentLoginViewModel),

        //    };
        //    var serializedData = JsonConvert.SerializeObject(secondApiRequest);
        //    var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
        //    if (response.IsSuccessStatusCode)
        //    {
        //        string data = response.Content.ReadAsStringAsync().Result;
        //        APIResponse<ProfessorHod> apiResponse = JsonConvert.DeserializeObject<APIResponse<ProfessorHod>>(data);
        //        if (apiResponse.IsSuccess && apiResponse.result != null)
        //        {
        //            professorHod = apiResponse.result;
        //            // Now you can work with the student object
        //        }
        //    }
        //    return professorHod;
        //}

        public JwtClaimsViewModel CheckLoginDetails(AdminStudentViewModel adminStudentViewModel)
        {
            string url = _httpClient.BaseAddress + "/Login/CheckLoginDetails/Login";
            JwtClaimsViewModel jwtClaimsViewModel = new();
            StudentLoginViewModel studentLoginViewModel = new()
            {
                UserName = adminStudentViewModel.UserName,
                Password = adminStudentViewModel.Password,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Login",
                MethodName = "CheckLoginDetails",
                DataObject = JsonConvert.SerializeObject(studentLoginViewModel),

            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<JwtClaimsViewModel> aPIResponse = JsonConvert.DeserializeObject<APIResponse<JwtClaimsViewModel>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    jwtClaimsViewModel = aPIResponse.result;
                }

            }
            return jwtClaimsViewModel;
        }

        public RoleBaseResponse<Student> GetAllStudents(string token, int RoleId)
        {
            string url = _httpClient.BaseAddress + "/Student/GetAllStudents/";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllStudents",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" }
            };
            _httpClient.DefaultRequestHeaders.Add("token", token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<Student> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<Student>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<Student>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
                else if (aPIResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    roleBaseResponse.IsAuthorize = false;
                }
            }
            return roleBaseResponse;
        }

        public RoleBaseResponse<Student> GetAllStudentsWithPagination(SecondApiRequest secondApi)
        {
            string url = _httpClient.BaseAddress + "/Student/GetAllStudents/";
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            PaginationViewModel paginationViewModel = new()
            {
                PageSize = secondApi.PageSize,
                StartIndex = secondApi.StartIndex,
                OrderBy = secondApi.OrderBy,
                OrderDirection = secondApi.OrderDirection,
                searchQuery = secondApi.searchQuery,
                JwtToken = secondApi.token,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetAllStudents",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = secondApi.RoleId,
                RoleIds = new List<string> { "1", "2" }

            };
            _httpClient.DefaultRequestHeaders.Add("token", secondApi.token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<Student> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<Student>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<Student>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                roleBaseResponse.IsAuthorize = false;
            }
            return roleBaseResponse;
        }


        public RoleBaseResponse<Book> GetAllBooksWithPagination(SecondApiRequest secondApi)
        {
            string url = _httpClient.BaseAddress + "/Book/GetAllBooks/";
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
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
                ControllerName = "Book",
                MethodName = "GetAllBooks",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllBooks",
                RoleId = secondApi.RoleId,
                RoleIds = new List<string> { "1" }

            };
            _httpClient.DefaultRequestHeaders.Add("token", secondApi.token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<Book> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<Book>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<Book>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
                else if (aPIResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    roleBaseResponse.IsAuthorize = false;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                roleBaseResponse.IsAuthorize = false;
            }
            return roleBaseResponse;
        }

        public Student GetStudentByMaster(int id, string token, SecondApiRequest apiRequest)
        {
            string url = _httpClient.BaseAddress + "/Student/GetStudent/";
            string cacheKey = "Student" + id;
            Student student = _cacheServices.GetSingleCachedResponse<Student>(cacheKey);
            if (student == null)
            {
                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Student",
                    MethodName = "GetStudent",
                    DataObject = JsonConvert.SerializeObject(id.ToString()),
                    MethodType = apiRequest.MethodType,
                    PageName = apiRequest.PageName,
                    RoleId = apiRequest.RoleId,
                    RoleIds = new List<string> { "1", "2", "3" }
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
                        _cacheServices.SetSingleCachedResponse(cacheKey, student);
                    }
                }
                return student;
            }
            return student;
        }

        public Book GetBook(int BookId, string token, SecondApiRequest apiRequest)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Book/GetBook/";
            Book book = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Book",
                MethodName = "GetBook",
                DataObject = JsonConvert.SerializeObject(BookId.ToString()),
                MethodType = apiRequest.MethodType,
                PageName = apiRequest.PageName,
                RoleId = apiRequest.RoleId,
                RoleIds = new List<string> { "1" }

            };
            _httpClient.DefaultRequestHeaders.Add("token", token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Book> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Book>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    book = aPIResponse.result;
                }

            }
            return book;
        }

        public IList<Course> GetAllCourses(string token, int RoleId)
        {
            string cacheKey = "Courses";
            IList<Course> cacheList = _cacheServices.GetListCachedResponse<Course>(cacheKey);
            if (cacheList == null)
            {
                string url = _httpClient.BaseAddress + "/Course/GetAllCourses/";
                if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
                {
                    HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                    if (apiVersionResponse.IsSuccessStatusCode)
                    {
                        string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                        SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                        if (settingdViewModel != null)
                        {
                            _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                        }
                    }
                }

                SecondApiRequest secondApiRequest = new()
                {
                    ControllerName = "Course",
                    MethodName = "GetAllCourses",
                    DataObject = JsonConvert.SerializeObject(null),
                    MethodType = "IsViewed",
                    PageName = "GetAllCourses",
                    RoleId = RoleId,
                    RoleIds = new List<string> { "1", "2" }

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
                    APIResponse<IList<Course>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<IList<Course>>>(data);
                    if (aPIResponse.IsSuccess && aPIResponse.result != null)
                    {
                        IList<Course> courses = aPIResponse.result;
                        _cacheServices.SetListCachedResponse<Course>(cacheKey, courses);
                        return courses;
                    }
                }

            }
            return cacheList;
        }

        public Course GetCourseDetailById(int id, int RoleId)
        {
            string url = _httpClient.BaseAddress + "/Course/GetCourse/";
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            Course course = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "GetCourse",
                DataObject = JsonConvert.SerializeObject(id.ToString()),
                MethodType = "IsViewed",
                PageName = "GetCourse",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" }

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
                RoleIds = new List<string> { "1", "2", "3" }

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
                RoleIds = new List<string> { "1", "2" }

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
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
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
                secondApiRequest.RoleIds = new List<string> { "1" };

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
                secondApiRequest.RoleIds = new List<string> { "1", "2" };

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

        public bool InsertCouse(Course course, int RoleId)
        {
            _httpClient.DefaultRequestHeaders.Add("token", course.JwtToken);
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Course/CreateCourse";
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Course",
                MethodName = "CreateCourse",
                DataObject = JsonConvert.SerializeObject(course),
                PageName = "CreateCourse",
                MethodType = "IsInsert",
                RoleId = RoleId,
                RoleIds = new List<string> { "1", "2" }
            };

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

        public async Task<bool> UpsertBook(BookViewModel bookViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (bookViewModel.PhotoFile != null)
            {
                IFormFile SingleFile = bookViewModel.PhotoFile;
                string fileName = Guid.NewGuid().ToString() + Path.GetFileName(SingleFile.FileName);
                //var filePath = Path.Combine("wwwroot", "BookPhotos", fileName);
                //using (FileStream stream = System.IO.File.Create(filePath))
                //{
                //    // The file is saved in a buffer before being processed
                //    await SingleFile.CopyToAsync(stream);
                //}
                bookViewModel.PhotoName = fileName;

                using var memoryStream = new MemoryStream();
                await SingleFile.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                bookViewModel.Photo = imageData;
            }
            _httpClient.DefaultRequestHeaders.Add("token", bookViewModel.JwtToken);
            string url = "";
            SecondApiRequest secondApiRequest = new();
            if (bookViewModel.BookId != 0)
            {
                url = _httpClient.BaseAddress + "/Book/UpdateBook";
                secondApiRequest.ControllerName = "Book";
                secondApiRequest.MethodName = "UpdateBook";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(bookViewModel);
                secondApiRequest.PageName = "UpsertBook";
                secondApiRequest.MethodType = "IsManaged";
                secondApiRequest.RoleId = bookViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1" };

            }
            else
            {
                url = _httpClient.BaseAddress + "/Book/CreateBook";
                secondApiRequest.ControllerName = "Book";
                secondApiRequest.MethodName = "CreateBook";
                secondApiRequest.DataObject = JsonConvert.SerializeObject(bookViewModel);
                secondApiRequest.PageName = "UpsertBook";
                secondApiRequest.MethodType = "IsInsert";
                secondApiRequest.RoleId = bookViewModel.RoleId;
                secondApiRequest.RoleIds = new List<string> { "1" };

            }
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Book> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Book>>(data);
                if (aPIResponse.IsSuccess)
                {
                    return true;
                }
            }
            return false;
        }

        public bool DeleteBook(BookViewModel bookViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Book/UpdateBook";
            SecondApiRequest secondApiRequest = new();
            secondApiRequest.ControllerName = "Book";
            secondApiRequest.MethodName = "DeleteBook";
            secondApiRequest.DataObject = JsonConvert.SerializeObject(bookViewModel);
            secondApiRequest.PageName = "DeleteBook";
            secondApiRequest.MethodType = "IsManaged";
            secondApiRequest.RoleId = bookViewModel.RoleId;
            secondApiRequest.RoleIds = new List<string> { "1" };

            _httpClient.DefaultRequestHeaders.Add("token", bookViewModel.JwtToken);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Book> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Book>>(data);
                if (aPIResponse.IsSuccess)
                {
                    return true;
                }
            }
            return false;
        }

        public Book GetBookPhoto(BookViewModel bookViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Book/GetBookPhoto/";
            Book book = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Book",
                MethodName = "GetBookPhoto",
                DataObject = JsonConvert.SerializeObject(bookViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllBooks",
                RoleId = bookViewModel.RoleId,
                RoleIds = new List<string> { "1", "2" },
            };
            _httpClient.DefaultRequestHeaders.Add("token", bookViewModel.JwtToken);

            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Book> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Book>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    book = aPIResponse.result;
                }
            }
            return book;
        }

        public string GetEmailFromStudentId(EmailViewModel emailViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", emailViewModel.JwtToken);
            }
            string url = _httpClient.BaseAddress + "/Student/GetEmailFromStudentId/";
            EmailViewModel emailViewModel1 = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetEmailFromStudentId",
                DataObject = JsonConvert.SerializeObject(emailViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = emailViewModel.RoleId,
                RoleIds = new List<string> { "1" }
            };

            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<EmailViewModel> aPIResponse = JsonConvert.DeserializeObject<APIResponse<EmailViewModel>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    emailViewModel1 = aPIResponse.result;
                }
            }
            return emailViewModel1.Email;
        }
        public List<StudentsEmailAndIds> GetEmailsAndIds(int? RoleId, string JwtToken)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Student/GetEmailsAndStudentIds/";
            List<StudentsEmailAndIds> list = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetEmailsAndStudentIds",
                DataObject = JsonConvert.SerializeObject(null),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" }

            };
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", JwtToken);

            }
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<List<StudentsEmailAndIds>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<List<StudentsEmailAndIds>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    list = aPIResponse.result;
                }
            }
            return list;
        }

        public EmailViewModel GetScheduledEmailById(int? RoleId, string JwtToken, int ScheduledEmailId)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            string url = _httpClient.BaseAddress + "/Email/GetScheduledEmailById/";
            EmailViewModel emailViewModel = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Email",
                MethodName = "GetScheduledEmailById",
                DataObject = JsonConvert.SerializeObject(ScheduledEmailId.ToString()),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = RoleId,
                RoleIds = new List<string> { "1" }

            };
            _httpClient.DefaultRequestHeaders.Add("token", JwtToken);

            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<EmailViewModel> aPIResponse = JsonConvert.DeserializeObject<APIResponse<EmailViewModel>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    emailViewModel = aPIResponse.result;
                }
            }
            return emailViewModel;
        }

        public async void SendEmail(EmailViewModel emailViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", emailViewModel.JwtToken);
            }
            string url = _httpClient.BaseAddress + "/ProfessorHod/SendEmail/";

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
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "ProfessorHod",
                MethodName = "SendEmail",
                DataObject = JsonConvert.SerializeObject(emailViewModel),
                MethodType = "IsViewed",
                PageName = "GetAllStudents",
                RoleId = emailViewModel.RoleId,
                RoleIds = new List<string> { "1" }
            };

            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            //if (response.IsSuccessStatusCode)
            //{
            //    string data = response.Content?.ReadAsStringAsync().Result;
            //    APIResponse<Student> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Student>>(data);
            //    if (aPIResponse.IsSuccess && aPIResponse.result != null)
            //    {

            //    }
            //}
        }

        public RoleBaseResponse<ScheduledEmailViewModel> GetAllScheduledEmail(SecondApiRequest secondApi)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", secondApi.token);
            }
            string url = _httpClient.BaseAddress + "/Email/GetScheduledEmails/";
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
                ControllerName = "Email",
                MethodName = "GetScheduledEmails",
                DataObject = JsonConvert.SerializeObject(paginationViewModel),
                MethodType = "IsViewed",
                PageName = "EmailLogs",
                RoleId = secondApi.RoleId,
                RoleIds = new List<string> { "1" }

            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<ScheduledEmailViewModel> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<ScheduledEmailViewModel>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<ScheduledEmailViewModel>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
                else if (aPIResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    roleBaseResponse.IsAuthorize = false;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                roleBaseResponse.IsAuthorize = false;
            }
            return roleBaseResponse;
        }

        public async void UpdateScheduledEmailLog(EmailViewModel emailViewModel)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", emailViewModel.JwtToken);
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
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
            SecondApiRequest secondApiRequest = new();
            string url = _httpClient.BaseAddress + "/Email/AddEditScheduledEmailLogs";
            secondApiRequest.ControllerName = "Email";
            secondApiRequest.MethodName = "AddEditScheduledEmailLogs";
            secondApiRequest.DataObject = JsonConvert.SerializeObject(emailViewModel);
            secondApiRequest.PageName = "EmailLogs";
            secondApiRequest.MethodType = "IsManaged";
            secondApiRequest.RoleId = emailViewModel.RoleId;
            secondApiRequest.RoleIds = new List<string> { "1" };

            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<EmailViewModel> aPIResponse = JsonConvert.DeserializeObject<APIResponse<EmailViewModel>>(data);
                if (aPIResponse.IsSuccess)
                {
                }
            }
        }

        public RoleBaseResponse<CountEmailViewModel> GetDayWiseEmailCount(int month, int year, int roleId, string jwtToken)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", jwtToken);
            }
            string url = _httpClient.BaseAddress + "/Email/GetDayWiseEmailCount/";
            CountEmailViewModel countEmailViewModel = new()
            {
                month = month,
                year = year,
            };
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Email",
                MethodName = "GetDayWiseEmailCount",
                DataObject = JsonConvert.SerializeObject(countEmailViewModel),
                MethodType = "IsViewed",
                PageName = "EmailLogs",
                RoleId = roleId,
                RoleIds = new List<string> { "1" }

            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<CountEmailViewModel> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<CountEmailViewModel>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<CountEmailViewModel>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
                else if (aPIResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    roleBaseResponse.IsAuthorize = false;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                roleBaseResponse.IsAuthorize = false;
            }
            return roleBaseResponse;
        }

        public RoleBaseResponse<CountStudentProfessor> GetDayWiseProfStudentCount(int month, int year, int roleId, string jwtToken)
        {
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")))
            {
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(apiVersionUrl).Result;
                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    SettingdViewModel settingdViewModel = JsonConvert.DeserializeObject<SettingdViewModel>(data);
                    if (settingdViewModel != null)
                    {
                        _httpClient.DefaultRequestHeaders.Add("Api-Version", settingdViewModel.SettingDescription);
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("token")))
            {
                _httpClient.DefaultRequestHeaders.Add("token", jwtToken);
            }
            string url = _httpClient.BaseAddress + "/Student/DayWiseCountStudentProf/";
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
                RoleId = roleId,
                RoleIds = new List<string> { "1" }

            };
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<CountStudentProfessor> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<CountStudentProfessor>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<CountStudentProfessor>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }
                else if (aPIResponse.StatusCode == HttpStatusCode.NonAuthoritativeInformation)
                {
                    roleBaseResponse.IsAuthorize = false;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                roleBaseResponse.IsAuthorize = false;
            }
            return roleBaseResponse;
        }

    }
}