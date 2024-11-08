using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.Models.DTO
{
    public class CourseCreateDto
    {
        [Required, StringLength(50)]
        public string CourseName { get; set; }

    }
}
