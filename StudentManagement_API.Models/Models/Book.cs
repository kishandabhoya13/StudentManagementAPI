using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.Models
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

        public string? Photo { get; set; } = null;

        public string? PhotoName { get; set; } = null;

        public int RowNumber { get; set; } = 0;

        public int TotalRecords { get; set; } = 0;
    }
}
