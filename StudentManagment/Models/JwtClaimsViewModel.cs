using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class JwtClaimsViewModel
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        public string Email { get; set; }

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public int RoleId { get; set; }

        public string? JwtToken { get; set; } = null;

        public bool IsConfirmed { get; set; }

        public bool IsRejected { get; set; }

        public bool IsBlocked { get; set; }
    }
}
