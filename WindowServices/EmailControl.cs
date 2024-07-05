using Microsoft.Extensions.Configuration;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WindowServices.Services;
using static DemoApiWithoutEF.Utilities.Enums;

namespace WindowServices
{
    internal class EmailControl
    {
        private readonly IConfiguration _configuration;
        private readonly IDbServices _dbServices;
        public EmailControl(IConfiguration configuration,IDbServices dbServices) 
        {
            _configuration= configuration;
            _dbServices= dbServices;
        }

        public void SendEmail()
        {
            IList<EmailLogs> emailLogs = DbClient.ExecuteProcedure<EmailLogs>("[dbo].[Get_Scheduled_Emails]", null);
            if (emailLogs.Count > 0)
            {
                foreach (var emailLog in emailLogs)
                {
                    MailMessage message = new MailMessage(_configuration["EmailCredential:From"], emailLog.Email);
                    SmtpClient client = new SmtpClient(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]));
                    System.Net.NetworkCredential basicCredential1 = new
                    System.Net.NetworkCredential(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = basicCredential1;
                    string mailbody = emailLog.Body;
                    message.Subject = emailLog.Subject;
                    message.Body = mailbody;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    try
                    {
                        client.Send(message);
                        _dbServices.AddEmailLogs(emailLog,true);
                        _dbServices.ChangeScheduledMailStatus(emailLog.ScheduledEmailId);
                    }
                    catch (Exception)
                    {
                        _dbServices.AddEmailLogs(emailLog,false);
                        throw;
                    }
                }
            }
        }
    }
}
