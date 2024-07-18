using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement_API.Models.Models
{
    public class EmailLogs
    {
        public int ScheduledEmailId { get; set; }

        [Key]
        public int EmailLogId { get; set; }

        public int? StudentId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTime? SentDate { get; set; }

        public int? SentBy { get; set; } = null;

        public string? Email { get; set; } = null;

        public bool IsSent { get; set; }

        public List<string> Emails { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int TotalRecords { get; set; }

        public int? DayWiseCount { get; set; } = 0;

        public int? year { get; set; } = 0;

        public int? month { get; set; } = 0;

        //public List<IFormFile>? AttachmentFiles { get; set; } = null;

        public Byte[]? AttachmentFile { get; set; } = null;

        public List<Byte[]>? AttachmentsByte { get; set; } = null;

        public Dictionary<string, byte[]>? FileNameWithAttachments { get; set; }

        public string? FileName { get; set; } = null;


    }

}
