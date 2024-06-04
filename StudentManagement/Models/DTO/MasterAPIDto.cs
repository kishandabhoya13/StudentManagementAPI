namespace StudentManagement_API.Models.DTO
{
    public class MasterAPIGetDto
    {
        public string token { get; set; } = null!;

        public string controllerName { get; set; } = null!;

        public string methodName { get; set; } = null!;

        public int? StudentId { get; set; } = null;

        public StudentLoginDto? StudentLoginDto { get; set; } = null;

    }

    public class MasterAPIPostDto
    {
        public StudentCreateDto? StudentCreateDto { get; set; } = null!;

        public StudentUpdateDto? StudentUpdateDto { get; set; } = null!;
    }

    public class MasterAPIIds
    {
        public int? StudentId { get; set; } = null;

        public int? CourseId { get; set; } = null;

    }

    public class ApiRequest
    {
        public string ControllerName { get; set; }

        public string MethodName { get; set; }

        public object DataObject { get; set; }

    }

}