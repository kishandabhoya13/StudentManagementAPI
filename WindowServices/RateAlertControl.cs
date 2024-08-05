using Microsoft.Extensions.Configuration;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Models.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using WindowServices.Services;

namespace WindowServices
{
    public class RateAlertControl
    {
        private readonly IDbServices _dbServices;
        private readonly IConfiguration _configuration;

        public RateAlertControl(IDbServices dbServices, IConfiguration configuration)
        {
            _dbServices = dbServices;
            _configuration = configuration;

        }

        public void SendRateAlertMail()
        {
            IList<CurrencyPairDto> currencyPairs = _dbServices.GetMatchedRateAlert();
            if(currencyPairs.Count > 0)
            {
                foreach (var alert in currencyPairs)
                {
                    MailMessage message = new MailMessage(_configuration["EmailCredential:From"], alert.Email);
                    SmtpClient client = new SmtpClient(_configuration["EmailCredential:Host"], int.Parse(_configuration["EmailCredential:Port"]));
                    System.Net.NetworkCredential basicCredential1 = new
                    System.Net.NetworkCredential(_configuration["EmailCredential:UserName"], _configuration["EmailCredential:PassWord"]);
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = basicCredential1;
                    string mailbody = "Today's Rate of "+alert.CurrencyPair.Substring(3) + " is matched with your exprected rate\n 1 "+alert.CurrencyPair.Substring(0,3)+" = "+ alert.Rate + " "+ alert.CurrencyPair.Substring(3);
                    message.Subject = "Expected Rates of Currency";
                    message.Body = mailbody;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    EmailLogs emailLog = new()
                    {
                        Body = message.Body,
                        Subject = message.Subject,
                        Email = alert.Email,
                        IsSent = true,
                        SentDate = DateTime.Now,
                        SentBy = 1,

                    };
                    try
                    {
                        client.Send(message);
                        int emailLogId = _dbServices.AddEmailLogs(emailLog, true);
                        _dbServices.UpdateRateAlert(alert.RateAlertId);
                    }
                    catch (Exception)
                    {
                        _dbServices.AddEmailLogs(emailLog, false);
                        throw;
                    }
                }
            }
        }
    }
}
