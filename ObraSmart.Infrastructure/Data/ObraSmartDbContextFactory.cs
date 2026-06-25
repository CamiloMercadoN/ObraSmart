using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ObraSmart.Infrastructure.Data
{
    // Esta clase intercepta a las herramientas de diseño(EF Power Tools, Migrations)
    // y les enseña cómo construir el DbContext sin depender del proyecto web.
    public class ObraSmartDbContextFactory : IDesignTimeDbContextFactory<ObraSmartDbContext>
    {
        public ObraSmartDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ObraSmartDbContext>();

            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=ObraSmartDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new ObraSmartDbContext(optionsBuilder.Options);
        }
    }
}
