using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentManagement.Models;
using StudentManagment.Models;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;
using StudentManagment.Services;

namespace StudentManagment.Controllers
{
    public class MasterController : Controller
    {
        Uri BaseAddress = new("https://localhost:7105/MasterApi");
        private readonly string apiVersionUrl = "https://localhost:7105/api/Login/GetApiVersion";
        private readonly HttpClient _httpClient;

        public MasterController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = BaseAddress;
        }
        

        public RoleBaseResponse<T> GetApiResponse<T>(SecondApiRequest apiRequest)
        {
           
            string url = _httpClient.BaseAddress + "/" + apiRequest.ControllerName+"/" + apiRequest.MethodName+"/";
            if (HttpContext.Session.GetInt32("RoleId") == 2 && apiRequest.MethodName != "UpdateProfessorHodJwtToken" && apiRequest.MethodName != "UpdateStudentJwtToken")
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                string url2 = "https://localhost:7105/api/Login/ProfessorDetails?UserId="+userId;
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(url2).Result;

                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    APIResponse<RoleBaseResponse<ProfessorHod>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<ProfessorHod>>>(data);
                    if (aPIResponse != null && aPIResponse.IsSuccess && aPIResponse.result != null)
                    {
                        if (aPIResponse.result.data.IsBlocked)
                        {
                            throw new UnauthorizedAccessException("Unauthorized access to API");
                        }
                    }
                }
                //string professorUrl = _httpClient.BaseAddress + "/Login/ProfessorDetails/Login";
                //SecondApiRequest aPIRequest2 = new SecondApiRequest()
                //{
                //    ControllerName = "Login",
                //    MethodName = "ProfessorDetails",
                //    DataObject = JsonConvert.SerializeObject(HttpContext.Session.GetInt32("UserId")),
                //};
                //var serializedData2 = JsonConvert.SerializeObject(aPIRequest2);
                //var content2 = new StringContent(serializedData2, Encoding.UTF8, "application/json");
                //HttpContext.Session.SetString("ApiCallTime", DateTime.Now.ToString());
                //HttpResponseMessage response2 = _httpClient.PostAsync(professorUrl, content2).Result;
                //RoleBaseResponse<T> roleBaseResponse2 = new();
                //if (response2.IsSuccessStatusCode)
                //{
                //    string data = response2.Content?.ReadAsStringAsync().Result;
                //    ProfessorHod aPIResponse2 = JsonConvert.DeserializeObject<ProfessorHod>(data);
                //    APIResponse<RoleBaseResponse<ProfessorHod>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<ProfessorHod>>>(data);
                //    if (aPIResponse.IsSuccess && aPIResponse.result != null)
                //    {
                //        if (aPIResponse.result.data.IsBlocked)
                //        {
                //            throw new UnauthorizedAccessException("Unauthorized access to API");
                //        }
                //    }
                //}
            }
            if (HttpContext.Session.GetInt32("RoleId") == 3 && apiRequest.MethodName != "UpdateProfessorHodJwtToken" && apiRequest.MethodName != "UpdateStudentJwtToken")
            {
                int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                string url2 = "https://localhost:7105/api/Login/StudentDetail?UserId=" + userId;
                HttpResponseMessage apiVersionResponse = _httpClient.GetAsync(url2).Result;

                if (apiVersionResponse.IsSuccessStatusCode)
                {
                    string data = apiVersionResponse.Content?.ReadAsStringAsync().Result;
                    APIResponse<RoleBaseResponse<ProfessorHod>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<ProfessorHod>>>(data);
                    if (aPIResponse != null && aPIResponse.IsSuccess && aPIResponse.result != null)
                    {
                        if (aPIResponse.result.data.IsBlocked)
                        {
                            throw new UnauthorizedAccessException("Unauthorized access to API");
                        }
                    }
                }
            }
            if (!(_httpClient.DefaultRequestHeaders.Contains("Api-Version")) && apiRequest.MethodName != "UpdateProfessorHodJwtToken" && apiRequest.MethodName != "UpdateStudentJwtToken")
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
                _httpClient.DefaultRequestHeaders.Add("token", apiRequest.token);

            }


            
            string date = DateTime.Now.ToString();
            HttpContext.Session.SetString("ApiCallTime", DateTime.Now.ToString());
            var serializedData = JsonConvert.SerializeObject(apiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<T> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<T>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<T>>>(data);
                if (aPIResponse.IsSuccess && aPIResponse.result != null)
                {
                    roleBaseResponse = aPIResponse.result;
                }else if(aPIResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new UnauthorizedAccessException("Unauthorized access to API");
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //roleBaseResponse.IsAuthorize = false;
                throw new UnauthorizedAccessException("Unauthorized access to API");
            }
            return roleBaseResponse;
        }

        public RoleBaseResponse<T> CallApiWithoutToken<T>(SecondApiRequest apiRequest)
        {
            string url = _httpClient.BaseAddress + "/" + apiRequest.ControllerName + "/" + apiRequest.MethodName + "/Login";
            var serializedData = JsonConvert.SerializeObject(apiRequest);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");
            HttpContext.Session.SetString("ApiCallTime", DateTime.Now.ToString());
            HttpResponseMessage response = _httpClient.PostAsync(url, content).Result;
            RoleBaseResponse<T> roleBaseResponse = new();
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content?.ReadAsStringAsync().Result;
                APIResponse<RoleBaseResponse<T>> aPIResponse = JsonConvert.DeserializeObject<APIResponse<RoleBaseResponse<T>>>(data);
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
    }
}
