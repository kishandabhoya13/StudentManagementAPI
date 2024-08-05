using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.Models
{
    public class ProfessorHod
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int RoleId { get; set; } 
        
        public string? JwtToken { get; set; } = null;

        public int TotalRecords { get; set; } = 0;

        public bool IsBlocked { get; set; }
    }
}
