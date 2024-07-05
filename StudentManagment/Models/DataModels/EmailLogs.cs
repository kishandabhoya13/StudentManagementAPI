using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models.DataModels
{
    public class EmailLogs
    {
        [Key]
        public int EmailLogId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public DateTime? SentDate { get; set; }

        public int? SentBy { get; set; } = null;
    }
}
