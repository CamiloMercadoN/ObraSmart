using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Application.DTOs.APUs;
using ObraSmart.Domain.Entities;
using ObraSmart.Infrastructure.Data;
using ObraSmart.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ObraSmart.IntegrationTests.Controllers
{
    public class EstructuraAPUControllerTests : IClassFixture<ObraSmartWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ObraSmartWebApplicationFactory _factory;

        public EstructuraAPUControllerTests(ObraSmartWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task CrearAPU_ConDatosValidos_DebeRetornar201Created()
        {
            // Arrange
            var usuarioTestId = Guid.Parse(TestAuthHandler.TestUsuarioId);
            var insumoTestId = Guid.NewGuid();

            // Preparamos la Base de Datos de Prueba inyectando el Usuario y el Insumo
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ObraSmartDbContext>();

                // Insertamos el usuario dueño de la sesión si no existe
                if (!db.Usuarios.Any(u => u.Id == usuarioTestId))
                {
                    db.Usuarios.Add(new Usuario
                    {
                        Id = usuarioTestId,
                        Rut = "11111111-1",
                        RazonSocial = "Usuario Test Integración",
                        Correo = "test@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15
                    });
                    await db.SaveChangesAsync();
                }

                // Insertamos el insumo asociado a ese usuario
                if (!db.Insumos.Any(i => i.Id == insumoTestId))
                {
                    db.Insumos.Add(new Insumo
                    {
                        Id = insumoTestId,
                        Descripcion = "Tubo PVC Test",
                        PrecioReferencia = 1500m,
                        UnidadMedidaId = 1,
                        TipoInsumo = "Material",
                        UsuarioId = usuarioTestId,
                        EsPlantilla = false
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Preparamos el DTO ahora con componentes válidos (cumpliendo la regla de negocio)
            var dto = new EstructuraAPUUpsertDto
            {
                Nombre = "Punto de Agua Potable PWA (Test Integración)",
                UnidadMedidaId = 1,
                EtiquetasIds = new List<Guid>(),
                Componentes = new List<ComponenteAPUInputDto>
                {
                    new ComponenteAPUInputDto { InsumoId = insumoTestId, Cantidad = 2 }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/apus", dto);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert HTTP
            response.StatusCode.Should().Be(
                HttpStatusCode.Created,
                $"la API respondió: {responseContent}");

            var createdId = JsonSerializer.Deserialize<Guid>(
                responseContent,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));

            createdId.Should().NotBeEmpty();

            // Assert persistencia
            await using var verificationScope =
                _factory.Services.CreateAsyncScope();

            var verificationDb = verificationScope.ServiceProvider
                .GetRequiredService<ObraSmartDbContext>();

            var estructuraCreada = await verificationDb.EstructurasAPU
                .IgnoreQueryFilters()
                .Include(e => e.Componentes)
                .SingleAsync(e => e.Id == createdId);

            estructuraCreada.UsuarioId.Should().Be(usuarioTestId);
            estructuraCreada.Nombre.Should().Be(dto.Nombre);
            estructuraCreada.Componentes.Should().ContainSingle();

            var componente = estructuraCreada.Componentes.Single();

            componente.InsumoId.Should().Be(insumoTestId);
            componente.Cantidad.Should().Be(2);

            // 2 unidades × $1.500
            estructuraCreada.CostoTotalCalculado.Should().Be(3000m);
        }
    }
}
