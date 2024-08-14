using System.ComponentModel.DataAnnotations;

namespace StudentManagment.Models
{
    public class QueriesViewModel
    {
        public QueriesViewModel()
        {
            QueriesReply = new List<QueriesViewModel>();
        }
        public int QueryId { get; set; }

        public string TicketNumber { get; set; }

        [Required(ErrorMessage = "Select Student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Enter Body")]
        public string Body { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Enter Subject")]
        public string Subject { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public IList<StudentsEmailAndIds> StudentsEmails { get; set; } 

        public List<QueriesViewModel> QueriesReply { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MessageId { get; set; }

        public bool IsSentMe { get; set; } = false;

    }

}
