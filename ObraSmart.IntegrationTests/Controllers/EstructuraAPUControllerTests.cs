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

        [Fact]
        public async Task CrearAPU_ConCantidadCero_DebeRetornar400BadRequestYNoPersistir()
        {
            // Arrange
            var nombreApu = $"APU Cantidad Cero {Guid.NewGuid()}";

            var dto = new EstructuraAPUUpsertDto
            {
                Nombre = nombreApu,
                UnidadMedidaId = 1,
                EtiquetasIds = new List<Guid>(),
                Componentes = new List<ComponenteAPUInputDto>
                {
                    new()
                    {
                        InsumoId = Guid.NewGuid(),
                        Cantidad = 0
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/apus", dto);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert HTTP y validación
            response.StatusCode.Should().Be(
                HttpStatusCode.BadRequest,
                $"la API respondió: {responseContent}");

            responseContent.Should().Contain(
                "La cantidad o rendimiento del insumo debe ser mayor a cero.");

            // Assert persistencia
            await using var verificationScope =
                _factory.Services.CreateAsyncScope();

            var verificationDb = verificationScope.ServiceProvider
                .GetRequiredService<ObraSmartDbContext>();

            var estructuraFuePersistida = await verificationDb.EstructurasAPU
                .IgnoreQueryFilters()
                .AnyAsync(e => e.Nombre == nombreApu);

            estructuraFuePersistida.Should().BeFalse(
                "una solicitud inválida no debe generar registros en la base de datos");
        }

        [Fact]
        public async Task ObtenerAPUs_ConRegistroDeOtroUsuario_DebeExcluirloDelResultado()
        {
            // Arrange
            var usuarioAutenticadoId = Guid.Parse(TestAuthHandler.TestUsuarioId);
            var otroUsuarioId = Guid.NewGuid();

            var apuPropiaId = Guid.NewGuid();
            var apuAjenaId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");

            var nombreApuPropia = $"APU propia {sufijo}";
            var nombreApuAjena = $"APU ajena {sufijo}";

            try
            {
                await using (var setupScope = _factory.Services.CreateAsyncScope())
                {
                    var db = setupScope.ServiceProvider
                        .GetRequiredService<ObraSmartDbContext>();

                    // El usuario autenticado puede haber sido creado por otra prueba.
                    var usuarioAutenticadoExiste = await db.Usuarios
                        .AnyAsync(u => u.Id == usuarioAutenticadoId);

                    if (!usuarioAutenticadoExiste)
                    {
                        db.Usuarios.Add(new Usuario
                        {
                            Id = usuarioAutenticadoId,
                            Rut = $"10{sufijo[..6]}-0",
                            RazonSocial = "Usuario autenticado de integración",
                            Correo = $"autenticado-{sufijo}@obrasmart.cl",
                            PasswordHash = "fake-hash",
                            PorcentajeIva = 19m,
                            ValidezCotizacionDias = 15
                        });
                    }

                    db.Usuarios.Add(new Usuario
                    {
                        Id = otroUsuarioId,
                        Rut = $"20{sufijo[..6]}-0",
                        RazonSocial = "Usuario ajeno de integración",
                        Correo = $"ajeno-{sufijo}@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15
                    });

                    db.EstructurasAPU.AddRange(
                        new EstructuraAPU
                        {
                            Id = apuPropiaId,
                            UsuarioId = usuarioAutenticadoId,
                            Nombre = nombreApuPropia,
                            UnidadMedidaId = 1,
                            CostoTotalCalculado = 1000m,
                            EsPlantilla = false
                        },
                        new EstructuraAPU
                        {
                            Id = apuAjenaId,
                            UsuarioId = otroUsuarioId,
                            Nombre = nombreApuAjena,
                            UnidadMedidaId = 1,
                            CostoTotalCalculado = 2000m,
                            EsPlantilla = false
                        });

                    await db.SaveChangesAsync();
                }

                // Act
                var response = await _client.GetAsync("/api/apus");

                var responseContent = await response.Content.ReadAsStringAsync();

                // Assert HTTP
                response.StatusCode.Should().Be(
                    HttpStatusCode.OK,
                    $"la API respondió: {responseContent}");

                var apus = await response.Content
                    .ReadFromJsonAsync<List<EstructuraAPUDto>>();

                apus.Should().NotBeNull();

                // Control positivo: el usuario sí puede consultar su propio registro.
                apus!.Should().Contain(
                    apu => apu.Id == apuPropiaId &&
                           apu.Nombre == nombreApuPropia);

                // Control de seguridad: el registro ajeno no debe ser visible.
                apus.Should().NotContain(
                    apu => apu.Id == apuAjenaId);

                apus.Should().NotContain(
                    apu => apu.Nombre == nombreApuAjena);
            }
            finally
            {
                // Limpieza para mantener la prueba independiente y repetible.
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var apusCreadas = await cleanupDb.EstructurasAPU
                    .IgnoreQueryFilters()
                    .Where(apu =>
                        apu.Id == apuPropiaId ||
                        apu.Id == apuAjenaId)
                    .ToListAsync();

                if (apusCreadas.Count != 0)
                {
                    cleanupDb.EstructurasAPU.RemoveRange(apusCreadas);
                    await cleanupDb.SaveChangesAsync();
                }

                var otroUsuario = await cleanupDb.Usuarios
                    .SingleOrDefaultAsync(u => u.Id == otroUsuarioId);

                if (otroUsuario is not null)
                {
                    cleanupDb.Usuarios.Remove(otroUsuario);
                    await cleanupDb.SaveChangesAsync();
                }
            }
        }

        [Fact]
        public async Task ObtenerAPUs_SinAutenticacion_DebeRetornar401Unauthorized()
        {
            // Arrange
            /*
             * Se crea un cliente diferente de _client.
             * Este cliente no recibe el encabezado Authorization
             * configurado en el constructor de la clase.
             */
            using var clientSinAutenticacion =
                _factory.CreateClient();

            // Act
            var response = await clientSinAutenticacion
                .GetAsync("/api/apus");

            // Assert
            response.StatusCode.Should().Be(
                HttpStatusCode.Unauthorized);
        }
    }
}
