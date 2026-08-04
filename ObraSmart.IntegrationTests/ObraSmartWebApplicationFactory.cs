using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Infrastructure.Data;
using ObraSmart.IntegrationTests.Helpers;
using Testcontainers.MsSql;
using Xunit;

namespace ObraSmart.IntegrationTests
{
    public class ObraSmartWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        // Define el contenedor efímero de SQL Server
        private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _msSqlContainer.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                // Remover la conexión a la base de datos de producción/desarrollo
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ObraSmartDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar el DbContext apuntando al contenedor Docker
                services.AddDbContext<ObraSmartDbContext>(options =>
                {
                    options.UseSqlServer(_msSqlContainer.GetConnectionString());
                });

                // Aplicar migraciones para crear las tablas automáticamente en el contenedor
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ObraSmartDbContext>();
                db.Database.Migrate();
                ObraSmartDbContextSeed.SeedAsync(db).GetAwaiter().GetResult();

                // Inyectar el esquema de autenticación falso
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            });
        }
    }
}
