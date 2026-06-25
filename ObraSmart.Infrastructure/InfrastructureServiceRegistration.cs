using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;
using ObraSmart.Infrastructure.Repositories;
using System.Reflection;

namespace ObraSmart.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registro del DbContext
            services.AddDbContext<ObraSmartDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Registro de Repositorios
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())// 1. Busca en este proyecto (Infrastructure)
            .AddClasses(classes =>
                classes.Where(type => type.Name.EndsWith("Repository")), // 2. Filtro: que terminen en "Repository"
                publicOnly: false) // 3. IMPORTANTE: false para incluir tus repos 'internal' también
            .AsImplementedInterfaces() // 4. Los vincula: Repository -> IRepository
            .WithScopedLifetime());    // 5. Ciclo de vida: Scoped (por petición HTTP)

            // A medida que crees nuevos repositorios (ej. IInsumoRepository), los agregas aquí.

            return services;
        }
    }
}
