using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models.DTO
{
    public class StudentLoginViewModel
    {
        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", ErrorMessage = "Password must have 8 characters, one uppercase, one lowercase, one digit, and one special character")]
        public string Password { get; set; } = null!;
    }

    public class SignUpViewModel
    {

        public int StudentId { get; set; } = 0;

        [RegularExpression("^[a-zA-Z]{1,50}$", ErrorMessage = "Only Alphabets are allowed")]
        [StringLength(50), Required(ErrorMessage = "FirstName is Required")]
        public string FirstName { get; set; }

        [RegularExpression("^[a-zA-Z]{1,50}$", ErrorMessage = "Only Alphabets are allowed")]
        [StringLength(50), Required(ErrorMessage = "LastName is Required")]
        public string LastName { get; set; }

        [RegularExpression("^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\\.[a-zA-Z0-9-.]+$", ErrorMessage = "Enter Correct Email")]
        [StringLength(50), Required, EmailAddress(ErrorMessage = "Enter Correct Email Address")]
        public string Email { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; } = null!;


        [StringLength(10)]
        [Required()]
        public string MobileNumber { get; set; }


        [StringLength(50)]
        [Required(ErrorMessage = "Password is Required")]
        [RegularExpression(pattern: "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$")]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "Enter Same Password and Confirmpassword")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "Date Of Birth is Required")]
        public DateTime? BirthDate { get; set; } = null;

        [Required(ErrorMessage = "Select Course")]
        public int CourseId { get; set; }

        public IList<Course> Courses { get; set; }

        public bool? IsConfirmed { get; set; } = null;

        public bool? IsRejected { get; set; } = null;

        public bool ApproveReject { get; set; }

        public string? Body { get; set; } = null;
    }

}
