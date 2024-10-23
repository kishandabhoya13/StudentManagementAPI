using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class HostDto
    {
        public int HostId { get; set; }
        public string AspNetUserId { get; set; }

        public string HostName { get; set; }

        public bool IsStarted { get; set; }

        public int TotalRecords { get; set; }
    }
}