using StudentManagment.Models;
using StudentManagment.Models.DataModels;
using System.Net;

namespace StudentManagement.Models
{
    public class APIResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess { get; set; } = true;

        public List<string> ErroMessages { get; set; }

        public T result { get; set; }
    }

    public class RoleBaseResponse<T>
    {
        public T data { get; set; }

        public IList<Course> Courses { get; set; }

        //public List<OrderByViewModel> OrderBys { get; set; }

        //public IList<EmailViewModel> AllEmails { get; set; }

        public T record{ get; set; }

        public string Role { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int StartIndex { get; set; } = 0;


        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; } = 0;

        public int TotalPages { get; set; }

        public bool IsAuthorize { get; set; } = true;

        public string? MonthName { get; set; } = null;

        public int? year { get; set; } = 0;
    }


    public class OrderByViewModel
    {
        public string OrderByValues{ get; set; }

        public string OrderByName { get; set; }
    }

    public class ApiVersionViewModel
    {
        public int ApiVersionId { get; set; }
        public string ApiVersionName { get; set;}
    }

    public class SettingdViewModel
    {
        public int SettingId { get; set; }

        public string SettingName { get; set; }

        public string SettingDescription { get; set; }
    }
}
