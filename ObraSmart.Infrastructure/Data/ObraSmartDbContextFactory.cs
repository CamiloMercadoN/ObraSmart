using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ObraSmart.Application.Interfaces.Services;

namespace ObraSmart.Infrastructure.Data
{
    // Esta clase intercepta a las herramientas de diseño(EF Power Tools, Migrations)
    // y les enseña cómo construir el DbContext sin depender del proyecto web.
    public class ObraSmartDbContextFactory : IDesignTimeDbContextFactory<ObraSmartDbContext>
    {
        public ObraSmartDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../ObraSmart.Server");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ObraSmartDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ObraSmartDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
        }

        // Clase privada (Dummy) exclusiva para engañar a Entity Framework durante las migraciones
        private class DesignTimeCurrentUserService : ICurrentUserService
        {
            public Guid? GetUserId()
            {
                // En tiempo de diseño (creando migraciones), no hay usuario conectado.
                return null;
            }
        }
    }
}
