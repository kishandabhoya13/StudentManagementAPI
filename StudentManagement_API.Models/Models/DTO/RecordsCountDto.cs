using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class RecordsCountDto
    {
        public int? StudentCount { get; set; } = 0;

        public int? BookCount { get; set; } = 0;

        public int? CourseCount { get; set; } = 0;

        public int? ScheduledCount { get; set; } = 0;

        public int? EmailCount { get; set; } = 0;

        public int? QueriesCount { get; set; } = 0;
    }
}
