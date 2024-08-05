using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentManagement.Models;
using StudentManagement_API.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StudentManagment.Models
{
    public class StudentViewModel
    {
        [Required]
        public int StudentId { get; set; }

        [StringLength(50), Required(ErrorMessage = "FirstName is Required")]
        public string FirstName { get; set; }

        [StringLength(50), Required(ErrorMessage = "LastName is Required")]
        public string LastName { get; set; }

        [Required]
        public DateTime? BirthDate { get; set; }

        [Required(ErrorMessage = "CourseId is Required")]
        public int CourseId { get; set; }

        public string? CourseName { get; set; } = null;

        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; } = null!;

        public string? JwtToken { get; set; } = null;

        public IList<Course> Courses { get; set; } = null;

        public int? RoleId { get; set; } = 0;

        public int CurrentPageNumber { get; set; }

        public int PageSize { get; set; } = 10;

        public string? searchQuery { get; set; } = null;

        public string? OrderBy { get; set; } = null;

        public string? OrderDirection { get; set; } = null;

        [RegularExpression("^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$", ErrorMessage = "Enter Correct Email")]
        [Required]
        public string Email { get; set; }

        public bool? IsConfirmed { get; set; } = null;

        public bool? IsRejected { get; set; } = null;
    }
}
