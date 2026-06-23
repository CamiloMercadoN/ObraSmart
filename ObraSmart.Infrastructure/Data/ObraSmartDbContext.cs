using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Infrastructure.Data
{
    public class ObraSmartDbContext : DbContext
    {
        public ObraSmartDbContext(DbContextOptions<ObraSmartDbContext> options)
            : base(options)
        {
        }

        // Definición de las Tablas
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<EstructuraAPU> EstructurasAPU { get; set; }
        public DbSet<ComponenteAPU> ComponentesAPU { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<ItemPresupuesto> ItemsPresupuesto { get; set; }
        public DbSet<RecursoItemPresupuesto> RecursosItemPresupuesto { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de precisión para campos Monetarios/Decimales (Vital para presupuestos)
            modelBuilder.Entity<Insumo>().Property(i => i.PrecioReferencia).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<EstructuraAPU>().Property(e => e.CostoTotalCalculado).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ComponenteAPU>().Property(c => c.Cantidad).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Presupuesto>().Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Presupuesto>().Property(p => p.MontoIva).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Presupuesto>().Property(p => p.Total).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ItemPresupuesto>().Property(i => i.CantidadItem).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ItemPresupuesto>().Property(i => i.PrecioUnitarioCalculado).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ItemPresupuesto>().Property(i => i.Subtotal).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<RecursoItemPresupuesto>().Property(r => r.Cantidad).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<RecursoItemPresupuesto>().Property(r => r.PrecioUnitarioCongelado).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<RecursoItemPresupuesto>().Property(r => r.CostoTotalRecurso).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Usuario>().Property(u => u.PorcentajeIva).HasColumnType("decimal(5,2)");

            // 2. Configurar la relación 1 a 1 entre Presupuesto y Cotización
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Cotizacion)
                .WithOne(c => c.Presupuesto)
                .HasForeignKey<Cotizacion>(c => c.PresupuestoId);


            // 3. Configuración para evitar eliminación en cascada en todas las relaciones
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
