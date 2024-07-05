using Microsoft.Extensions.Configuration;
using NLog;
using StudentManagement_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WindowServices.Services;

namespace WindowServices
{
    public static class CompositionRoot
    {
        private static readonly IUnityContainer Container = RegisterDependencies(new UnityContainer());

        public static void DisposeContainer()
        {
            Container.Dispose();
        }

        public static TService Resolve<TService>()
        {
            return Resolve<TService>(null);
        }

        public static TService Resolve<TService>(string name)
        {
            return Container.Resolve<TService>(name);
        }

        private static IUnityContainer RegisterDependencies(IUnityContainer container)
        {
            container.RegisterInstance(LogManager.Configuration);

            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var configurationBuilder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.Development.json");

                var configuration = configurationBuilder.Build();
                container.RegisterInstance<IConfiguration>(configuration);
                container.RegisterSingleton<IDbServices,DbServices>();

                return container;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Error configurating the service dependencies.");
                throw;
            }
        }
    }
}
