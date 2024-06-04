using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "character varying")]
        public string Name { get; set; } = null!;
    }
}
