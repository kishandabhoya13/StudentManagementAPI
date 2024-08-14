using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class QueriesDto
    {
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

        public int TotalRecords { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MessageId { get; set; }
    }
}
