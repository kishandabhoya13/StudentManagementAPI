using Microsoft.Extensions.DependencyInjection.Extensions;
using NetCore.AutoRegisterDi;
using System.Reflection;

namespace StudentManagment
{
    public static class DataLayerServicesRegistration
    {
        public static IServiceCollection AddDataLayerServices(this IServiceCollection services)
        {
            services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly())
                    .Where(x => x.Name.EndsWith("Services"))
                    .AsPublicImplementedInterfaces().ToArray();

            return services;
        }
    }
}
