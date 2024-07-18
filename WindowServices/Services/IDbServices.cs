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
        int AddEmailLogs(EmailLogs emailLog,bool IsSent);

        IList<EmailLogs> GetAttachementsFromScheduledId(int scheduledId);

        void ChangeScheduledMailStatus(int scheduledEmailId);

        bool IsPDF(byte[] bytes);

        void UpdateAttachmentEmailLogId(IList<EmailLogs> attachments, int emailLogId);
    }
}
