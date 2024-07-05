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
}
