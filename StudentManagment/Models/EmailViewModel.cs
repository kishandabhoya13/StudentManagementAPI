using StudentManagment.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class EmailViewModel
    {
        [Key]
        public int EmailLogId { get; set; }

        [Required(ErrorMessage ="Select Email")]
        public int StudentId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTime? SentDate { get; set; }

        public int? SentBy { get; set; } = null;

        public string? FirstName { get; set; } = null;

        public string? LastName { get; set; } = null;

        public IList<StudentsEmailAndIds>? StudentsEmails { get; set; } = null;

        public int? RoleId { get; set; } = null;

        public string? JwtToken { get; set; } = null;

        public string? Email { get; set; } = null;

        public List<string> Emails { get; set; }

        public int ScheduledEmailId { get; set; }

        public bool IsSent { get; set; }

        public int? month { get; set; } = 0;

        public int? DayWiseDayWiseCount { get; set; } = 0;

        public List<IFormFile>? AttachmentFiles { get; set; } = null;

        public List<Byte[]>? AttachmentsByte { get; set; } = null;

        public Dictionary<string, byte[]>? FileNameWithAttachments { get; set; }
    }
}

public class StudentsEmailAndIds
{
    public int StudentId { get; set; }  

    public string Email { get; set; }   

    public string FirstName { get; set; }   

    public string LastName { get; set; }
}


public class ScheduledEmailLogs
{
    public IList<EmailViewModel> AllEmails { get; set; }
}


public class ScheduledEmailViewModel
{
    public int? StudentId { get; set; } = null;

    public string Subject { get; set; }


    public DateTime? SentDate { get; set; }

    public int? SentBy { get; set; } = null;

    public string? FirstName { get; set; } = null;

    public string? LastName { get; set; } = null;


    public int? RoleId { get; set; } = null;

    public string? JwtToken { get; set; } = null;

    public string? Email { get; set; } = null;


    public int ScheduledEmailId { get; set; }

    public bool IsSent { get; set; }
}

public class CountEmailViewModel
{
    public DateTime? SentDate { get; set; }

    public int? DayWiseCount { get; set; } = 0;

    public int? month { get; set; } = 0;

    public int? year { get; set; } = 0;

}