using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentManagement.Models;
using StudentManagment.Models;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

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
                _httpClient.DefaultRequestHeaders.Add("token", apiRequest.token);

            }
            //PaginationViewModel paginationViewModel = new()
            //{
            //    PageSize = secondApi.PageSize,
            //    StartIndex = secondApi.StartIndex,
            //    OrderBy = secondApi.OrderBy,
            //    OrderDirection = secondApi.OrderDirection,
            //    searchQuery = secondApi.searchQuery,
            //    JwtToken = secondApi.token,
            //    Id = secondApi.Id,
            //};
            //SecondApiRequest secondApiRequest = new()
            //{
            //    ControllerName = apiRequest.ControllerName,
            //    MethodName = apiRequest.MethodName,
            //    DataObject = JsonConvert.SerializeObject(apiRequest.dataObject),
            //    MethodType = apiRequest.MethodType,
            //    PageName = apiRequest.PageName,
            //    RoleId = apiRequest.RoleId,
            //    RoleIds = apiRequest.RoleIds

            //};
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
