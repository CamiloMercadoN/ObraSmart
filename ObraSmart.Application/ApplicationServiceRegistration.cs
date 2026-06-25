using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Application.Settings;
using System.Reflection;

namespace ObraSmart.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConfiguracionNegocio>(configuration.GetSection("ConfiguracionNegocio"));

            // Registro Automático de Servicios de Negocio (Scrutor)
            // Busca todas las clases que terminen en "Service"
            services.Scan(scan => scan
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }
    }
}
