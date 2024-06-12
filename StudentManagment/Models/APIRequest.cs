

using static StudentManagment.Models.APIMethodType;

namespace StudentManagment.Models
{
    public class APIRequest
    {
        public APIType ApiType { get; set; } = APIType.GET;

        public string url { get; set; }

        public object Data { get; set; }
    }

    public class SecondApiRequest
    {
        public string ControllerName { get; set; }

        public string MethodName { get; set; }

        public string DataObject { get; set; }

        public string? PageName { get; set; }

        public int? RoleId { get; set; } = null;

        public string? MethodType { get; set; } = null;

        //public int CurrentPageNumber { get; set; } = 0;
        public int StartIndex { get; set; } = 0;


        public int PageSize { get; set; } = 10;

        public string? searchQuery { get; set; } = null;

        public string? OrderBy { get; set; } = null;

        public string? OrderDirection { get; set; } = null;

        public string token { get; set; }


    }

    public class UpdateJwtViewModel
    {
        public string token { get; set; }

        public int Id { get; set; }
    }
}
