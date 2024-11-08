using NLog;
using NLog.Config;
using Topshelf;
using Topshelf.HostConfigurators;

namespace WindowServices
{
    internal class Program
    {
        protected Program() { }

        public static void Main(string[] args)
        {
            HostFactory.Run(HostConfiguration);
        }

        private static void HostConfiguration(HostConfigurator config)
        {
            config.RunAsLocalService();
            config.Service(CompositionRoot.Resolve<ServicesControl>, c => c.AfterStoppingService(CompositionRoot.DisposeContainer));
            config.StartManually();

            var logFactory = CompositionRoot.Resolve<LogFactory>();
            LogManager.Configuration = new XmlLoggingConfiguration("nLog.config");
            LogManager.GetLogger("Default").Info("NLog configured");

            config.StartManually();


        }
    }
}
