using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;
using ObraSmart.Infrastructure.Repositories;
using ObraSmart.Infrastructure.Services;
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
            //services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.Scan(scan => scan
            .FromAssemblies(Assembly.GetExecutingAssembly())// Busca en este proyecto (Infrastructure)
            .AddClasses(classes =>
                classes.Where(type => type.Name.EndsWith("Repository") || type.Name.EndsWith("Service")), // Filtro: que terminen en "Repository" y "Service"
                publicOnly: false) // false para incluir tus repos 'internal' también
            .AsImplementedInterfaces() // Los vincula: Repository -> IRepository
            .WithScopedLifetime());    // Ciclo de vida: Scoped (por petición HTTP)

            // Registro del repositorio genérico abierto (Scoped)
            services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));

            services.AddScoped<IFileService, LocalFileService>();

            return services;
        }
    }
}
