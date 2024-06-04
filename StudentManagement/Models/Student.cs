using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime? BirthDate { get; set; }
        
        public int CourseId { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? JwtToken { get; set; } = null;

    }
}
