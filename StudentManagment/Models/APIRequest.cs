

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

        public object DataObject { get; set; }

    }
}
