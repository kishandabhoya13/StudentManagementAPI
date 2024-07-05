using Microsoft.Extensions.Configuration;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
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

        public void AddEmailLogs(EmailLogs emailLog,bool IsSent)
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
            DbClient.ExecuteProcedure(EmailLogSql, parameters, ExecuteType.ExecuteNonQuery);
        }

        public void ChangeScheduledMailStatus(int scheduledEmailId)
        {
            string Sql = "[dbo].[Update_Scheduled_Emails_IsSent]";

            Collection<DbParameters> parameters = new Collection<DbParameters>();
            parameters.Add(new DbParameters() { Name = "@scheduledEmailId", Value = scheduledEmailId, DBType = DbType.Int64});
            DbClient.ExecuteProcedure(Sql, parameters, ExecuteType.ExecuteNonQuery);
        }
    }
}
