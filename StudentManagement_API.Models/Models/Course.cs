using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement_API.Models.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Column(TypeName = "character varying")]
        public string CourseName { get; set; } = null!;
    }
}
