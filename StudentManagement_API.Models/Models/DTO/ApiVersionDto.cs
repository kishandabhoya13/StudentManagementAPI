using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagement_API.Models.Models.DTO
{
    public class ApiVersionDto
    {
        public int ApiVersionId { get; set; }
        public string ApiVersionName { get; set; }
    }

    public class SettingDto
    {
        public int SettingId { get; set; }

        public string SettingName { get; set; }

        public string SettingDescription { get; set; }

        public bool IsBlocked { get; set; }
    }
}