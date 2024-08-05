using Microsoft.Extensions.Configuration;
using NLog;
using StudentManagement_API.DataContext;
using StudentManagement_API.Models.Models;
using StudentManagement_API.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;
using WindowServices.Services;
using Timer = System.Timers.Timer;

namespace WindowServices
{
    internal class ServicesControl : ServiceControl, IDisposable
    {
        private Timer timer;
        private Timer timer2;
        private Timer timer3;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IConfiguration _configuration;
        private readonly IDbServices _dbServices;
        public ServicesControl(IConfiguration configuration,IDbServices dbServices)
        {
            _configuration= configuration;
            _dbServices= dbServices;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Dispose();
            }
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var emailControl = new EmailControl(_configuration,_dbServices);
                var rateAlertControl = new RateAlertControl(_dbServices,_configuration);
                emailControl.SendEmail();
                rateAlertControl.SendRateAlertMail();
            }
            catch(Exception ex)
            {
                string stackTrace = !string.IsNullOrEmpty(ex.StackTrace) ? ex.StackTrace : "";
                string source = !string.IsNullOrEmpty(ex.Source) ? ex.Source : "";
                string exceptionMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : "";
                string message = "Start - Error running Rate Alert service , StackTrace : " + stackTrace + " , Source : " + source + " , Exception Message : " + exceptionMessage;
                logger.Fatal(ex, message);
            }

            var runIntervalString = System.Configuration.ConfigurationManager.AppSettings["RunInterval"];
            TimeSpan runInterval;
            if(!TimeSpan.TryParseExact(runIntervalString,"g",CultureInfo.InvariantCulture,out runInterval))
            {
                return false;
            }
            timer = new Timer(runInterval.TotalMilliseconds);
            timer.Elapsed += this.OnTimedEvent;
            timer.Enabled = true;

            var runIntervalString2 = System.Configuration.ConfigurationManager.AppSettings["RunInterval2"];
            TimeSpan runInterval2;
            if (!TimeSpan.TryParseExact(runIntervalString2, "g", CultureInfo.InvariantCulture, out runInterval2))
            {
                return false;
            }
            timer2 = new Timer(runInterval2.TotalMilliseconds);
            timer2.Elapsed += this.OnTimedSecondEvent;
            timer2.Enabled = true;

            var runIntervalString3 = System.Configuration.ConfigurationManager.AppSettings["RunInterval3"];
            TimeSpan runInterval3;
            if (!TimeSpan.TryParseExact(runIntervalString3, "g", CultureInfo.InvariantCulture, out runInterval3))
            {
                return false;
            }
            timer3 = new Timer(runInterval3.TotalMilliseconds);
            timer3.Elapsed += this.OnTimedThirdEvent;
            timer3.Enabled = true;

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            timer.Stop();
            return true;
        }

        private void OnTimedEvent(object state, ElapsedEventArgs e)
        {
            timer.Stop();
            this.RunTasks();
            timer.Start();

            
        }

        private void OnTimedSecondEvent(object state, ElapsedEventArgs e)
        {
            timer2.Stop();
            logger.Info("Second Sevice Started");
            this.RunSecondTasks();
            logger.Info("Second Sevice Finished");
            timer2.Start();
        }

        private void OnTimedThirdEvent(object state, ElapsedEventArgs e)
        {
            timer3.Stop();
            this.RunThirdTasks();
            timer3.Start();


        }

        private void RunTasks()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var emailControl = new EmailControl(_configuration,_dbServices);
                emailControl.SendEmail();
            }
            catch (Exception ex)
            {
                string stackTrace = !string.IsNullOrEmpty(ex.StackTrace) ? ex.StackTrace : "";
                string source = !string.IsNullOrEmpty(ex.Source) ? ex.Source : "";
                string exceptionMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : "";
                string message = "Start - Error running Rate Alert service , StackTrace : " + stackTrace + " , Source : " + source + " , Exception Message : " + exceptionMessage;
                logger.Fatal(ex, message);
            }
        }

        private void RunSecondTasks()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                logger.Info("Second Service is doing background work.");
            }
            catch (Exception ex)
            {
                string stackTrace = !string.IsNullOrEmpty(ex.StackTrace) ? ex.StackTrace : "";
                string source = !string.IsNullOrEmpty(ex.Source) ? ex.Source : "";
                string exceptionMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : "";
                string message = "Start - Error running Rate Alert service , StackTrace : " + stackTrace + " , Source : " + source + " , Exception Message : " + exceptionMessage;
                logger.Fatal(ex, message);
            }
        }

        private void RunThirdTasks()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var rateAlertControl = new RateAlertControl(_dbServices, _configuration);
                rateAlertControl.SendRateAlertMail();
            }
            catch (Exception ex)
            {
                string stackTrace = !string.IsNullOrEmpty(ex.StackTrace) ? ex.StackTrace : "";
                string source = !string.IsNullOrEmpty(ex.Source) ? ex.Source : "";
                string exceptionMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : "";
                string message = "Start - Error running Rate Alert service , StackTrace : " + stackTrace + " , Source : " + source + " , Exception Message : " + exceptionMessage;
                logger.Fatal(ex, message);
            }
        }
    }
}
