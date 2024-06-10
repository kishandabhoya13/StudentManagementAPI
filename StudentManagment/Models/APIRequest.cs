

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

    }

    public class UpdateJwtViewModel
    {
        public string token { get; set; }

        public int StudentId { get; set; }
    }
}
