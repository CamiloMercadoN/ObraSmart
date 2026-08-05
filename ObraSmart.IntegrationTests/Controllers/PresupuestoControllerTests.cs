using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Application.DTOs.Presupuestos;
using ObraSmart.Domain.Entities;
using ObraSmart.Infrastructure.Data;
using ObraSmart.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ObraSmart.IntegrationTests.Controllers
{
    public class PresupuestoControllerTests
        : IClassFixture<ObraSmartWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ObraSmartWebApplicationFactory _factory;

        public PresupuestoControllerTests(
            ObraSmartWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task CrearPresupuesto_ConDatosValidos_DebePersistirTotalesYRecursosHistoricos()
        {
            // Arrange
            var usuarioTestId =
                Guid.Parse(TestAuthHandler.TestUsuarioId);

            var sufijo = Guid.NewGuid().ToString("N");
            var nombreProyecto =
                $"Presupuesto integración {sufijo}";

            Guid presupuestoCreadoId = Guid.Empty;

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider.GetRequiredService<ObraSmartDbContext>();

                var usuarioExiste = await db.Usuarios.AnyAsync(u => u.Id == usuarioTestId);

                if (!usuarioExiste)
                {
                    db.Usuarios.Add(new Usuario
                    {
                        Id = usuarioTestId,
                        Rut = "11111111-1",
                        RazonSocial =
                            "Usuario Test Integración",
                        Correo = "test@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15
                    });

                    await db.SaveChangesAsync();
                }
            }

            var dto = new PresupuestoUpsertDto
            {
                ClienteId = null,
                NombreProyecto = nombreProyecto,
                Items = new List<ItemPresupuestoUpsertDto>
                {
                    new()
                    {
                        Descripcion ="Instalación de lavaplatos",
                        CantidadItem = 2m,
                        UnidadMedidaId = 1,
                        Recursos = new List<RecursoItemPresupuestoUpsertDto>
                            {
                                new()
                                {
                                    TipoInsumo = "Material",
                                    DescripcionCongelada =
                                        "Tubería PPR 20 mm",
                                    Cantidad = 3m,
                                    PrecioUnitarioCongelado =
                                        1000.50m,
                                    UnidadMedidaId = 1
                                },
                                new()
                                {
                                    TipoInsumo = "ManoObra",
                                    DescripcionCongelada =
                                        "Maestro gasfíter",
                                    Cantidad = 1.5m,
                                    PrecioUnitarioCongelado =
                                        2000.25m,
                                    UnidadMedidaId = 1
                                }
                            }
                    },
                    new()
                    {
                        Descripcion = "Prueba de funcionamiento",
                        CantidadItem = 1.5m,
                        UnidadMedidaId = 1,
                        Recursos = new List<RecursoItemPresupuestoUpsertDto>
                            {
                                new()
                                {
                                    TipoInsumo = "Equipo",
                                    DescripcionCongelada =
                                        "Equipo de prueba",
                                    Cantidad = 2m,
                                    PrecioUnitarioCongelado =
                                        500.10m,
                                    UnidadMedidaId = 1
                                }
                            }
                    }
                }
            };

            try
            {
                // Act
                var response = await _client.PostAsJsonAsync("/api/presupuestos", dto);

                var responseContent = await response.Content.ReadAsStringAsync();

                // Assert HTTP
                response.StatusCode.Should().Be(HttpStatusCode.Created, $"la API respondió: {responseContent}");

                presupuestoCreadoId = JsonSerializer.Deserialize<Guid>(responseContent, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                presupuestoCreadoId.Should().NotBeEmpty();

                // Assert persistencia
                await using var verificationScope = _factory.Services.CreateAsyncScope();

                var verificationDb = verificationScope.ServiceProvider.GetRequiredService<ObraSmartDbContext>();

                var presupuestoCreado =
                    await verificationDb.Presupuestos
                        .IgnoreQueryFilters()
                        .Include(p => p.Items)
                        .ThenInclude(i => i.Recursos)
                        .SingleAsync(
                            p => p.Id == presupuestoCreadoId);

                presupuestoCreado.UsuarioId.Should().Be(usuarioTestId);

                presupuestoCreado.NombreProyecto.Should().Be(nombreProyecto);

                presupuestoCreado.ClienteId.Should().BeNull();

                presupuestoCreado.Estado.Should().Be("Borrador");

                presupuestoCreado.EsPlantilla.Should().BeFalse();

                presupuestoCreado.FechaCreacion.Should().NotBe(default);

                presupuestoCreado.Items.Should().HaveCount(2);

                /*
                 * Ítem 1:
                 *
                 * Material:
                 * 3 × 1.000,50 = 3.001,50
                 *
                 * Mano de obra:
                 * 1,5 × 2.000,25 = 3.000,375
                 * Redondeado        = 3.000,38
                 *
                 * Precio unitario   = 6.001,88
                 * Cantidad ítem     = 2
                 * Subtotal ítem     = 12.003,76
                 *
                 * Ítem 2:
                 *
                 * Equipo:
                 * 2 × 500,10        = 1.000,20
                 *
                 * Cantidad ítem     = 1,5
                 * Subtotal ítem     = 1.500,30
                 *
                 * Subtotal general  = 13.504,06
                 * IVA 19 %          =  2.565,77
                 * Total             = 16.069,83
                 */

                presupuestoCreado.Subtotal.Should().Be(13504.06m);

                presupuestoCreado.MontoIva.Should().Be(2565.77m);

                presupuestoCreado.Total.Should().Be(16069.83m);

                var itemInstalacion =
                    presupuestoCreado.Items.Single(
                        i => i.Descripcion ==
                            "Instalación de lavaplatos");

                itemInstalacion.CantidadItem.Should().Be(2m);

                itemInstalacion.PrecioUnitarioCalculado.Should().Be(6001.88m);

                itemInstalacion.Subtotal.Should().Be(12003.76m);

                itemInstalacion.Recursos.Should().HaveCount(2);

                var material =itemInstalacion.Recursos.Single(r => r.DescripcionCongelada == "Tubería PPR 20 mm");

                material.TipoInsumo.Should().Be("Material");

                material.Cantidad.Should().Be(3m);

                material.PrecioUnitarioCongelado.Should().Be(1000.50m);

                material.CostoTotalRecurso.Should().Be(3001.50m);

                var manoObra = itemInstalacion.Recursos.Single( r => r.DescripcionCongelada == "Maestro gasfíter");

                manoObra.TipoInsumo.Should().Be("ManoObra");

                manoObra.Cantidad.Should().Be(1.5m);

                manoObra.PrecioUnitarioCongelado.Should().Be(2000.25m);

                manoObra.CostoTotalRecurso.Should().Be(3000.38m);

                var itemPrueba = presupuestoCreado.Items.Single(i => i.Descripcion == "Prueba de funcionamiento");

                itemPrueba.CantidadItem.Should().Be(1.5m);

                itemPrueba.PrecioUnitarioCalculado.Should().Be(1000.20m);

                itemPrueba.Subtotal.Should().Be(1500.30m);

                var equipo = itemPrueba.Recursos
                        .Should()
                        .ContainSingle()
                        .Subject;

                equipo.DescripcionCongelada.Should().Be("Equipo de prueba");

                equipo.PrecioUnitarioCongelado.Should().Be(500.10m);

                equipo.CostoTotalRecurso.Should().Be(1000.20m);
            }
            finally
            {
                if (presupuestoCreadoId != Guid.Empty)
                {
                    await using var cleanupScope = _factory.Services.CreateAsyncScope();

                    var cleanupDb = cleanupScope.ServiceProvider.GetRequiredService<ObraSmartDbContext>();

                    var presupuesto =
                        await cleanupDb.Presupuestos
                            .IgnoreQueryFilters()
                            .Include(p => p.Items)
                            .ThenInclude(i => i.Recursos)
                            .SingleOrDefaultAsync(
                                p => p.Id ==
                                    presupuestoCreadoId);

                    if (presupuesto is not null)
                    {
                        var itemIds = presupuesto.Items.Select(i => i.Id).ToList();

                        if (itemIds.Count > 0)
                        {
                            await cleanupDb
                                .RecursosItemPresupuesto
                                .Where(r =>
                                    itemIds.Contains(
                                        r.ItemPresupuestoId))
                                .ExecuteDeleteAsync();

                            await cleanupDb.ItemsPresupuesto
                                .Where(i =>
                                    itemIds.Contains(i.Id))
                                .ExecuteDeleteAsync();
                        }

                        await cleanupDb.Presupuestos
                            .IgnoreQueryFilters()
                            .Where(p =>
                                p.Id == presupuestoCreadoId)
                            .ExecuteDeleteAsync();
                    }
                }
            }
        }
    }
}