//using StudentManagement_API.DataContext;
//using StudentManagement_API.Models.Models;
//using StudentManagement_API.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mail;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Helpers;

//namespace SendEmailServices
//{
//    public class SendEmail : BackgroundService
//    {

//        public ILogger Logger { get; set; }
//        private readonly IConfiguration _configuration;
//        public SendEmail(ILoggerFactory loggerFactory, IConfiguration configuration)
//        {
//            Logger = loggerFactory.CreateLogger<SendEmail>();
//            _configuration = configuration;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stopppingToken)
//        {
//            Logger.LogInformation("SendEmail Sevice Started");

//            stopppingToken.Register(() => Logger.LogInformation("SendEmail Service is Stoppping"));

//            while (!stopppingToken.IsCancellationRequested)
//            {

//                Logger.LogInformation("SendEmail is doing background work.");
//                IList<EmailLogs> emailLogs = DbClient.ExecuteProcedure<EmailLogs>("[dbo].[Get_Scheduled_Emails]", null);
//                if (emailLogs.Count > 0)
//                {
//                    foreach (var emailLog in emailLogs)
//                    {
//                        MailMessage message = new MailMessage(_configuration["EmailCredential:From"], emailLog.Email);
//                        SmtpClient client = new SmtpClient(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]));
//                        System.Net.NetworkCredential basicCredential1 = new
//                        System.Net.NetworkCredential(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
//                        client.EnableSsl = true;
//                        client.UseDefaultCredentials = false;
//                        client.Credentials = basicCredential1;
//                        string mailbody = emailLog.Body;
//                        message.Subject = emailLog.Subject;
//                        message.Body = mailbody;
//                        message.BodyEncoding = Encoding.UTF8;
//                        message.IsBodyHtml = true;
//                        try
//                        {
//                            client.Send(message);
//                            string EmailLogSql = "[dbo].[Add_EmailLog_Details]";
//                            EmailLogs newEmailLog = new()
//                            {
//                                Body = emailLog.Body,
//                                Subject = emailLog.Subject,
//                                Email = emailLog.Email,
//                                IsSent = true,
//                                SentDate = emailLog.SentDate,
//                                SentBy = emailLog.SentBy,

//                            };
//                            _studentServices.AddEmailLogs(newEmailLog, EmailLogSql);
//                        }
//                        catch (Exception)
//                        {
//                            string EmailLogSql = "[dbo].[Add_EmailLog_Details]";
//                            EmailLogs newEmailLog = new()
//                            {
//                                Body = emailLog.Body,
//                                Subject = emailLog.Subject,
//                                Email = emailLog.Email,
//                                IsSent = true,
//                                SentDate = emailLog.SentDate,
//                                SentBy = emailLog.SentBy,

//                            };
//                            _studentServices.AddEmailLogs(newEmailLog, EmailLogSql);
//                            throw;
//                        }
//                    }
//                }
//                await Task.Delay(TimeSpan.FromHours(24), stopppingToken);
//            }

//            Logger.LogInformation("SendEmail has stopped.");
//        }
//    }
//}
