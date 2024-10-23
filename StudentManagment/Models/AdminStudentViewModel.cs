using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class AdminStudentViewModel
    {
        [Key]
        public int Id { get; set; }

        [Key]
        public int StudentId { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [StringLength(50)]
        public string LastName { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        public string UserName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public int RoleId { get; set; }

        public string? JwtToken { get; set; } = null;

       
    }

    public class CountStudentProfessor
    {
        public DateTime? CreatedDate1 { get; set; }

        public DateTime? CreatedDate2 { get; set; }

        public int? StudentDayWiseCount { get; set; }

        public int? ProfessorDayWiseCount { get; set; }

        public int? month { get; set; } = 0;

        public int? year { get; set; } = 0;
    }

    public class ForgotPasswordViewModel
    {
        public string? Email { get; set; } = null;

        [Required(ErrorMessage = "Enter UserName or Email")]
        public string UserName { get; set; } 

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Password and Confirm Password Must be Same"), Required(ErrorMessage = "Enter Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        public string? ResetToken { get; set; } = null;

        public DateTime? ExpirationTime { get; set; } = null;

        public bool IsFirstTime { get; set; } = false;

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string OldPassword { get; set; }

        public int StudentId { get; set; }

    }
}
