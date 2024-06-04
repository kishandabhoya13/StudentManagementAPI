using Newtonsoft.Json;
using StudentManagement.Models;
using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using StudentManagment.Services.Interface;
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
            _httpClient.BaseAddress = BaseAddress;
            //_httpClient.BaseAddress = SecondAddress;
        }

        public Student CheckLoginDetails(StudentViewModel studentViewModel)
        {
            string url = _httpClient.BaseAddress + "/Student/CheckLogin?UserName=" + studentViewModel.UserName + "&Password=" + studentViewModel.Password;
            Student student = new();
            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
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

        public Student GetStudentByMaster(int id,string token)
        {
            string url = "https://localhost:7105/MasterApi/Student/GetStudent/" + id;
            Student student = new();
            SecondApiRequest secondApiRequest = new()
            {
                ControllerName = "Student",
                MethodName = "GetStudent",
                DataObject = 1,
            };
            _httpClient.DefaultRequestHeaders.Add("token", token);
            var serializedData = JsonConvert.SerializeObject(secondApiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url,content).Result;
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

        public Student GetStudentDetailById(int id,string token)
        {
            string url = _httpClient.BaseAddress + "/Student/" + id;
            Student student = new();
            _httpClient.DefaultRequestHeaders.Add("token", token);
            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<Student> aPIResponse = JsonConvert.DeserializeObject<APIResponse<Student>>(data);
                if(aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    student = aPIResponse.result;
                }
            }
            return student;
        }

        public Course GetCourseDetailById(int id)
        {
            string url = _httpClient.BaseAddress + "/Course/" + id;
            Course course = new();
            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
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

        public bool UpdateJwtToken(string token,int StudentId)
        {
            string url = _httpClient.BaseAddress + "/Student/UpdateJwtToken?StudentId=" + StudentId;
            string data = JsonConvert.SerializeObject(token);
            StringContent content= new StringContent(data,Encoding.UTF8,"application/json");
            HttpResponseMessage response = _httpClient.PutAsync(url,content).Result;    
            if(response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

    }
}
