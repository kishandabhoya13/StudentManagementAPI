﻿using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.Models.DTO
{
    public class StudentUpdateDto
    {
        [Key]
        [Required]
        public int StudentId { get; set; }

        [StringLength(50), Required(ErrorMessage = "FirstName is Required")]
        public string FirstName { get; set; } = null!;

        [StringLength(50), Required(ErrorMessage = "LastName is Required")]
        public string LastName { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "CourseId is Required")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; } = null!;

        [RegularExpression("^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$", ErrorMessage = "Enter Correct Email")]
        [Required]
        public string Email { get; set; }

        public bool? IsConfirmed { get; set; } = null;

        public bool? IsRejected { get; set; } = null;

        public bool ApproveReject { get; set; }

        public string? Body { get; set; } = null;
    }
}
