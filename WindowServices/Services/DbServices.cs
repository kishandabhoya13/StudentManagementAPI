using Microsoft.Extensions.Configuration;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static DemoApiWithoutEF.Utilities.Enums;

namespace WindowServices.Services
{
    public class DbServices : IDbServices
    {
        public DbServices()
        {
        }

        public int AddEmailLogs(EmailLogs emailLog, bool IsSent)
        {
            string EmailLogSql = "[dbo].[Add_EmailLog_Details]";
            EmailLogs newEmailLog = new()
            {
                Body = emailLog.Body,
                Subject = emailLog.Subject,
                Email = emailLog.Email,
                IsSent = IsSent,
                SentDate = emailLog.SentDate,
                SentBy = emailLog.SentBy,

            };
            var table = new DataTable();
            table.Columns.Add("Email");
            table.Columns.Add("Subject");
            table.Columns.Add("Body");
            table.Columns.Add("SentDate");
            table.Columns.Add("SentBy");
            table.Columns.Add("IsSent");


            var row = table.NewRow();
            row["Email"] = newEmailLog.Email;
            row["Subject"] = newEmailLog.Subject;
            row["Body"] = newEmailLog.Body;
            row["SentDate"] = DateTime.Now.ToString("MM-dd-yyyy hh:mm:ss");
            row["SentBy"] = newEmailLog.SentBy;
            row["IsSent"] = newEmailLog.IsSent;
            table.Rows.Add(row);

            Collection<DbParameters> parameters = new Collection<DbParameters>();

            parameters.Add(new DbParameters() { Name = "@email_details", Value = table, DBType = DbType.Object, TypeName = "[Email_log_details]" });
            parameters.Add(new DbParameters() { Name = "@emailLogId", DBType = DbType.Int64, Direction = ParameterDirection.Output });
            EmailLogs emailLogs = DbClient.ExecuteOneRecordProcedure<EmailLogs>(EmailLogSql, parameters);
            return emailLogs.EmailLogId;
        }

        public IList<EmailLogs> GetAttachementsFromScheduledId(int scheduledId)
        {
            string Sql = "[dbo].[Get_Attachment_By_ScheduledEmailId]";

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailid", Value = scheduledId, DBType = DbType.Int64 });
            return DbClient.ExecuteProcedure<EmailLogs>(Sql, parameters);
        }

        public void ChangeScheduledMailStatus(int scheduledEmailId)
        {
            string Sql = "[dbo].[Update_Scheduled_Emails_IsSent]";

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailId", Value = scheduledEmailId, DBType = DbType.Int64 });
            DbClient.ExecuteProcedure(Sql, parameters, ExecuteType.ExecuteNonQuery);
        }

        public void UpdateAttachmentEmailLogId(IList<EmailLogs> attachments,int emailLogId)
        {
            string Sql = "[dbo].[Update_EmailAttachments_EmailLogsId]";
            var table = new DataTable();
            table.Columns.Add("ScheduledEmailId");
            table.Columns.Add("EmailLogsId");

            foreach(var attachment in attachments)
            {
                if(attachment.AttachmentFile != null)
                {
                    var row = table.NewRow();
                    row["ScheduledEmailId"] = attachment.ScheduledEmailId;
                    row["EmailLogsId"] = emailLogId;
                    table.Rows.Add(row);
                }
            }

            Collection<DbParameters> parameters = new Collection<DbParameters>();

            parameters.Add(new DbParameters() { Name = "@attachmentTableType", Value = table, DBType = DbType.Object, TypeName = "[AttachmentTableType]" });
            DbClient.ExecuteProcedure(Sql, parameters, ExecuteType.ExecuteNonQuery);
        }

        public bool IsPDF(byte[] bytes)
        {
            byte[] PDFSignature = { 37, 80, 68, 70, 45, 49, 46 };
            if (bytes.Length >= PDFSignature.Length &&
         bytes.Take(PDFSignature.Length).SequenceEqual(PDFSignature))
            {
                return true;
            }
            return false;
        }

        public IList<CurrencyPairDto> GetMatchedRateAlert()
        {
            string Sql = "[dbo].[Get_Matched_RateAlert]";
            return DbClient.ExecuteProcedure<CurrencyPairDto>(Sql, null);
        }

        public void UpdateRateAlert(int RateAlertId)
        {
            string query = "UPDATE RateAlerts SET IsCompleted = 1 where RateAlertId =" + RateAlertId;
            DbClient.ExecuteProcedureWithQuery(query, null, ExecuteType.ExecuteNonQuery);
        }
    }
}
