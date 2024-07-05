using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models.DataModels
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        public string? BookNumber { get; set; } = null;

        [Required,StringLength(50)]
        public string BookTitle { get; set; }

        [Required(ErrorMessage = "Please Select Course")]
        public int CourseId { get; set; }

        public string? CourseName { get; set; } = null;

        [Required,StringLength(100)]
        public string Subject { get; set; }

        public byte[] Photo { get; set; }

        public string? Photos { get; set; }

        public string? PhotoName { get; set; } = null;


        public IFormFile PhotoFile { get; set; }

        public bool IsDeleted { get; set; } = false;

    }
}
