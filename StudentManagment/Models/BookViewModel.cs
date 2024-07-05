using StudentManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class BookViewModel
    {
        [Key]
        public int BookId { get; set; }

        public string? BookNumber { get; set; } = null;

        [Required, StringLength(50)]
        public string BookTitle { get; set; }

        [Required(ErrorMessage = "Please Select Course")]
        public int CourseId { get; set; }

        public string? CourseName { get; set; } = null;

        [Required, StringLength(100)]
        public string Subject { get; set; }

        public byte[]? Photo { get; set; } = null;

        public string? PhotoName { get; set; } = null;

        public IFormFile? PhotoFile { get; set; } = null;

        public IList<Course>? Courses { get; set; } = null;

        public string? JwtToken { get; set; } = null;

        public int? RoleId { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;
    }
}
