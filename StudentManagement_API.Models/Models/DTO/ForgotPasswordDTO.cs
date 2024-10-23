using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class ForgotPasswordDTO
    {
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter UserName or Email")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Password and Confirm Password Must be Same"), Required(ErrorMessage = "Enter Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;

        public string? ResetToken { get; set; } = null;

        public DateTime? ExpirationTime { get; set; } = null;

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
        public int StudentId { get; set; }
    }
}
