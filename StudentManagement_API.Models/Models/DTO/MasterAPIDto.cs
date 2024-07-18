using Microsoft.AspNetCore.Http;

namespace StudentManagement_API.Models.Models.DTO
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

        public string DataObject { get; set; }

        public string? PageName { get; set; }

        public int? RoleId { get; set; } = null;

        public List<string>? RoleIds { get; set; } = null;

        public string? MethodType { get; set; } = null;

    }

    public class UpdateJwtDTo
    {
        public string token { get; set; }

        public int Id { get; set; }
    }


    public class CountStudentProfessorDto
    {
        public DateTime? CreatedDate1 { get; set; } = null;

        public DateTime? CreatedDate2 { get; set; } = null;

        public int? StudentDayWiseCount { get; set; } = null;

        public int? ProfessorDayWiseCount { get; set; } = null;

        public int? month { get; set; } = 0;

        public int? year { get; set; } = 0;
    }
}