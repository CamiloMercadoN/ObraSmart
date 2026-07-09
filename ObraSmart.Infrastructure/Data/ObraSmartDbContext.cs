using Microsoft.EntityFrameworkCore;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces;
using System.Reflection;

namespace ObraSmart.Infrastructure.Data
{
    public class ObraSmartDbContext(DbContextOptions<ObraSmartDbContext> options, ICurrentUserService currentUserService) : DbContext(options)
    {

        private readonly ICurrentUserService _currentUserService = currentUserService;

        public Guid? CurrentUserId => _currentUserService.GetUserId();

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
        public DbSet<Pais> Paises { get; set; }
        public DbSet<EstadoProvincia> EstadoProvincias { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión para campos Monetarios/Decimales (Vital para presupuestos)
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

            // Configurar la relación 1 a 1 entre Presupuesto y Cotización
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Cotizacion)
                .WithOne(c => c.Presupuesto)
                .HasForeignKey<Cotizacion>(c => c.PresupuestoId);


            // Configuración para evitar eliminación en cascada en todas las relaciones
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Configuración de filtros globales para entidades que implementan IUserOwnedEntity
            var userOwnedEntities = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(IUserOwnedEntity).IsAssignableFrom(e.ClrType));

            foreach (var entity in userOwnedEntities)
            {
                var method = typeof(ObraSmartDbContext)
                    .GetMethod(nameof(AplicarFiltroUsuario), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(entity.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }

        private void AplicarFiltroUsuario<T>(ModelBuilder modelBuilder) where T : class, IUserOwnedEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => e.UsuarioId == CurrentUserId);
        }
    }
}
