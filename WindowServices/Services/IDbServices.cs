using StudentManagement_API.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowServices.Services
{
    public interface IDbServices
    {
        void AddEmailLogs(EmailLogs emailLog,bool IsSent);

        void ChangeScheduledMailStatus(int scheduledEmailId);
    }
}
