using System.Net;

namespace StudentManagement_API.Models
{
    public class APIResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess { get; set; } = true;
        
        public List<string> ErroMessages { get; set; }

        public string? JwtToken { get; set; } = null;

        public object result { get; set; }
    }

    public class RoleBaseResponse
    {
        public List<Student> Students { get; set; }

        public string? Role { get; set;}
    }
}
