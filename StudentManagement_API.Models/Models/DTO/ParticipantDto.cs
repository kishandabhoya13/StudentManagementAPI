using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class ParticipantDto
    {
        public int ParticipantId { get; set; }

        public string ParticipantName { get; set; }

        public string AspNetUserId { get; set; }

        public int HostId { get; set; }
    }
}
