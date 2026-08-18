using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Domain.Entities;
using ObraSmart.Infrastructure.Data;
using ObraSmart.IntegrationTests.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ObraSmart.IntegrationTests.Controllers
{
    public class CotizacionControllerTests
        : IClassFixture<ObraSmartWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly ObraSmartWebApplicationFactory _factory;

        public CotizacionControllerTests(
            ObraSmartWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task CrearCotizacion_ConPresupuestoValido_DebeRetornar201YPersistir()
        {
            // Arrange
            var usuarioTestId =
                Guid.Parse(TestAuthHandler.TestUsuarioId);

            var clienteId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();
            var cotizacionCreadaId = Guid.Empty;

            var sufijo = Guid.NewGuid().ToString("N");

            int ultimoNumeroOriginal = 0;

            await using (var setupScope =
                _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var usuario = await db.Usuarios
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(
                        u => u.Id == usuarioTestId);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        Id = usuarioTestId,
                        Rut = $"9{sufijo[..7]}-K",
                        RazonSocial = "Usuario Test Integración",
                        Correo = $"test-{sufijo}@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15,
                        UltimoNumeroCotizacion = 0
                    };

                    db.Usuarios.Add(usuario);
                    await db.SaveChangesAsync();
                }

                ultimoNumeroOriginal =
                    usuario.UltimoNumeroCotizacion;

                var cliente = new Cliente
                {
                    Id = clienteId,
                    UsuarioId = usuarioTestId,
                    Nombre = $"Cliente integración {sufijo}",
                    Rut = $"8{sufijo[..7]}-K",
                    Correo = $"cliente-{sufijo}@test.cl",
                    Telefono = "+56912345678",
                    Direccion = "Dirección integración",
                    EsPlantilla = false
                };

                db.Clientes.Add(cliente);

                var presupuesto = new Presupuesto
                {
                    Id = presupuestoId,
                    UsuarioId = usuarioTestId,
                    ClienteId = clienteId,
                    NombreProyecto =
                        $"Presupuesto cotización {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                db.Presupuestos.Add(presupuesto);

                await db.SaveChangesAsync();
            }

            var fechaVencimiento =
                DateTime.UtcNow.Date.AddDays(15);

            var dto = new CrearCotizacionRequestDto
            {
                PresupuestoId = presupuestoId,
                FechaVencimiento = fechaVencimiento
            };

            try
            {
                // Act
                var response =
                    await _client.PostAsJsonAsync(
                        "/api/cotizaciones",
                        dto);

                var responseContent =
                    await response.Content.ReadAsStringAsync();

                // Assert HTTP
                response.StatusCode.Should().Be(
                    HttpStatusCode.Created,
                    $"la API respondió: {responseContent}");

                using var json =
                    JsonDocument.Parse(responseContent);

                var root = json.RootElement;

                cotizacionCreadaId =
                    root.GetProperty("id").GetGuid();

                var numeroCotizacion =
                    root.GetProperty("numeroCotizacion")
                        .GetString();

                var estado =
                    root.GetProperty("estado")
                        .GetString();

                cotizacionCreadaId.Should().NotBeEmpty();

                numeroCotizacion.Should().Be(
                    $"COT-{ultimoNumeroOriginal + 1}");

                estado.Should().Be("Borrador");

                // Assert persistencia
                await using var verificationScope =
                    _factory.Services.CreateAsyncScope();

                var verificationDb =
                    verificationScope.ServiceProvider
                        .GetRequiredService<ObraSmartDbContext>();

                var cotizacionCreada =
                    await verificationDb.Cotizaciones
                        .AsNoTracking()
                        .SingleAsync(
                            c => c.Id == cotizacionCreadaId);

                cotizacionCreada.PresupuestoId
                    .Should().Be(presupuestoId);

                cotizacionCreada.NumeroCotizacion
                    .Should().Be(
                        $"COT-{ultimoNumeroOriginal + 1}");

                cotizacionCreada.Estado
                    .Should().Be("Borrador");

                cotizacionCreada.FechaEmision
                    .Should().NotBe(default);

                cotizacionCreada.FechaVencimiento.Date
                    .Should().Be(fechaVencimiento.Date);

                cotizacionCreada.ArchivoPdfUrl
                    .Should().BeEmpty();

                var usuarioActualizado =
                    await verificationDb.Usuarios
                        .AsNoTracking()
                        .SingleAsync(
                            u => u.Id == usuarioTestId);

                usuarioActualizado
                    .UltimoNumeroCotizacion
                    .Should()
                    .Be(ultimoNumeroOriginal + 1);
            }
            finally
            {
                // Cleanup
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb =
                    cleanupScope.ServiceProvider
                        .GetRequiredService<ObraSmartDbContext>();

                if (cotizacionCreadaId != Guid.Empty)
                {
                    await cleanupDb.Cotizaciones
                        .Where(
                            c => c.Id ==
                                cotizacionCreadaId)
                        .ExecuteDeleteAsync();
                }

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == presupuestoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == clienteId)
                    .ExecuteDeleteAsync();

                var usuario =
                    await cleanupDb.Usuarios
                        .SingleAsync(
                            u => u.Id == usuarioTestId);

                usuario.UltimoNumeroCotizacion =
                    ultimoNumeroOriginal;

                await cleanupDb.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task DescargarPdf_DesdeBorrador_DebeRetornarPdfYMarcarCotizacionComoEmitida()
        {
            // Arrange
            var usuarioTestId = Guid.Parse(TestAuthHandler.TestUsuarioId);

            var clienteId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();
            var cotizacionId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");
            var numeroCotizacion = $"COT-TEST-{sufijo[..8]}";

            string? archivoPdfUrl = null;

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var usuario = await db.Usuarios
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(u => u.Id == usuarioTestId);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        Id = usuarioTestId,
                        Rut = $"9{sufijo[..7]}-K",
                        RazonSocial = "Usuario Test Integración",
                        Correo = $"test-{sufijo}@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15,
                        UltimoNumeroCotizacion = 0
                    };

                    db.Usuarios.Add(usuario);
                }

                var cliente = new Cliente
                {
                    Id = clienteId,
                    UsuarioId = usuarioTestId,
                    Nombre = $"Cliente PDF {sufijo}",
                    Rut = $"8{sufijo[..7]}-K",
                    Correo = $"cliente-pdf-{sufijo}@test.cl",
                    Telefono = "+56912345678",
                    Direccion = "Dirección prueba PDF",
                    EsPlantilla = false
                };

                var presupuesto = new Presupuesto
                {
                    Id = presupuestoId,
                    UsuarioId = usuarioTestId,
                    ClienteId = clienteId,
                    NombreProyecto = $"Presupuesto PDF {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                var cotizacion = new Cotizacion
                {
                    Id = cotizacionId,
                    PresupuestoId = presupuestoId,
                    NumeroCotizacion = numeroCotizacion,
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                db.Clientes.Add(cliente);
                db.Presupuestos.Add(presupuesto);
                db.Cotizaciones.Add(cotizacion);

                await db.SaveChangesAsync();
            }

            try
            {
                // Act
                var response = await _client.GetAsync(
                    $"/api/cotizaciones/{cotizacionId}/pdf");

                var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                // Assert HTTP
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                response.Content.Headers.ContentType
                    .Should().NotBeNull();

                response.Content.Headers.ContentType!.MediaType
                    .Should().Be("application/pdf");

                response.Content.Headers.ContentDisposition
                    .Should().NotBeNull();

                response.Content.Headers.ContentDisposition!.FileName
                    .Should().Contain(numeroCotizacion);

                // Assert PDF
                pdfBytes.Should().NotBeNull();
                pdfBytes.Should().NotBeEmpty();
                pdfBytes.Length.Should().BeGreaterThan(100);

                var encabezadoPdf = System.Text.Encoding.ASCII
                    .GetString(pdfBytes.Take(5).ToArray());

                encabezadoPdf.Should().Be("%PDF-");

                // Assert persistencia
                await using var verificationScope =
                    _factory.Services.CreateAsyncScope();

                var verificationDb = verificationScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionActualizada = await verificationDb.Cotizaciones
                    .AsNoTracking()
                    .SingleAsync(c => c.Id == cotizacionId);

                var presupuestoActualizado = await verificationDb.Presupuestos
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == presupuestoId);

                cotizacionActualizada.Estado
                    .Should().Be("Emitida");

                cotizacionActualizada.ArchivoPdfUrl
                    .Should().NotBeNullOrWhiteSpace();

                presupuestoActualizado.Estado
                    .Should().Be("Emitido");

                archivoPdfUrl = cotizacionActualizada.ArchivoPdfUrl;

                // Assert archivo físico
                var rutaArchivo = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Storage",
                    archivoPdfUrl!.Replace(
                        "/",
                        Path.DirectorySeparatorChar.ToString()));

                File.Exists(rutaArchivo)
                    .Should().BeTrue();

                var bytesGuardados = await File.ReadAllBytesAsync(rutaArchivo);

                bytesGuardados.Should().Equal(pdfBytes);
            }
            finally
            {
                // Cleanup archivo físico
                if (!string.IsNullOrWhiteSpace(archivoPdfUrl))
                {
                    var rutaArchivo = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Storage",
                        archivoPdfUrl.Replace(
                            "/",
                            Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(rutaArchivo))
                    {
                        File.Delete(rutaArchivo);
                    }
                }

                // Cleanup base de datos
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                await cleanupDb.Cotizaciones
                    .Where(c => c.Id == cotizacionId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == presupuestoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == clienteId)
                    .ExecuteDeleteAsync();
            }
        }

        [Fact]
        public async Task DescargarPdf_SinAutenticacion_DebeRetornar401Unauthorized()
        {
            // Arrange
            var usuarioTestId = Guid.Parse(TestAuthHandler.TestUsuarioId);

            var clienteId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();
            var cotizacionId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");
            var numeroCotizacion = $"COT-SEC-{sufijo[..8]}";

            string? archivoPdfUrl = null;

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var usuario = await db.Usuarios
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(u => u.Id == usuarioTestId);

                if (usuario == null)
                {
                    usuario = new Usuario
                    {
                        Id = usuarioTestId,
                        Rut = $"9{sufijo[..7]}-K",
                        RazonSocial = "Usuario Test Integración",
                        Correo = $"test-{sufijo}@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15,
                        UltimoNumeroCotizacion = 0
                    };

                    db.Usuarios.Add(usuario);
                }

                var cliente = new Cliente
                {
                    Id = clienteId,
                    UsuarioId = usuarioTestId,
                    Nombre = $"Cliente Seguridad {sufijo}",
                    Rut = $"8{sufijo[..7]}-K",
                    Correo = $"cliente-seguridad-{sufijo}@test.cl",
                    Telefono = "+56912345678",
                    Direccion = "Dirección prueba seguridad",
                    EsPlantilla = false
                };

                var presupuesto = new Presupuesto
                {
                    Id = presupuestoId,
                    UsuarioId = usuarioTestId,
                    ClienteId = clienteId,
                    NombreProyecto = $"Presupuesto Seguridad {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                var cotizacion = new Cotizacion
                {
                    Id = cotizacionId,
                    PresupuestoId = presupuestoId,
                    NumeroCotizacion = numeroCotizacion,
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                db.Clientes.Add(cliente);
                db.Presupuestos.Add(presupuesto);
                db.Cotizaciones.Add(cotizacion);

                await db.SaveChangesAsync();
            }

            try
            {
                // Cliente SIN Authorization: TestScheme
                using var clienteNoAutenticado = _factory.CreateClient();

                // Act
                var response = await clienteNoAutenticado.GetAsync(
                    $"/api/cotizaciones/{cotizacionId}/pdf");

                // Assert HTTP
                response.StatusCode.Should().Be(
                    HttpStatusCode.Unauthorized);

                // Assert: la solicitud anónima no debe producir efectos secundarios
                await using var verificationScope =
                    _factory.Services.CreateAsyncScope();

                var verificationDb = verificationScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionPersistida = await verificationDb.Cotizaciones
                    .AsNoTracking()
                    .SingleAsync(c => c.Id == cotizacionId);

                var presupuestoPersistido = await verificationDb.Presupuestos
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == presupuestoId);

                cotizacionPersistida.Estado
                    .Should().Be("Borrador");

                cotizacionPersistida.ArchivoPdfUrl
                    .Should().BeEmpty();

                presupuestoPersistido.Estado
                    .Should().Be("Borrador");

                archivoPdfUrl = cotizacionPersistida.ArchivoPdfUrl;
            }
            finally
            {
                // Si la prueba descubre el fallo de seguridad y el PDF
                // alcanzó a generarse, eliminamos también el archivo físico.
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionPersistida = await cleanupDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.Id == cotizacionId);

                if (cotizacionPersistida != null &&
                    !string.IsNullOrWhiteSpace(cotizacionPersistida.ArchivoPdfUrl))
                {
                    archivoPdfUrl = cotizacionPersistida.ArchivoPdfUrl;
                }

                if (!string.IsNullOrWhiteSpace(archivoPdfUrl))
                {
                    var rutaArchivo = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Storage",
                        archivoPdfUrl.Replace(
                            "/",
                            Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(rutaArchivo))
                    {
                        File.Delete(rutaArchivo);
                    }
                }

                await cleanupDb.Cotizaciones
                    .Where(c => c.Id == cotizacionId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == presupuestoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == clienteId)
                    .ExecuteDeleteAsync();
            }
        }

        [Fact]
        public async Task DescargarPdf_DeOtroUsuario_DebeDenegarAccesoYNoModificarDatos()
        {
            // Arrange
            var usuarioAutenticadoId = Guid.Parse(TestAuthHandler.TestUsuarioId);

            var otroUsuarioId = Guid.NewGuid();
            var clienteAjenoId = Guid.NewGuid();
            var presupuestoAjenoId = Guid.NewGuid();
            var cotizacionAjenaId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");
            var numeroCotizacion = $"COT-AJENA-{sufijo[..8]}";

            string? archivoPdfUrl = null;

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var otroUsuario = new Usuario
                {
                    Id = otroUsuarioId,
                    Rut = $"7{sufijo[..7]}-K",
                    RazonSocial = "Usuario Ajeno Integración",
                    Correo = $"usuario-ajeno-{sufijo}@obrasmart.cl",
                    PasswordHash = "fake-hash",
                    PorcentajeIva = 19m,
                    ValidezCotizacionDias = 15,
                    UltimoNumeroCotizacion = 1
                };

                var clienteAjeno = new Cliente
                {
                    Id = clienteAjenoId,
                    UsuarioId = otroUsuarioId,
                    Nombre = $"Cliente Ajeno {sufijo}",
                    Rut = $"6{sufijo[..7]}-K",
                    Correo = $"cliente-ajeno-{sufijo}@test.cl",
                    Telefono = "+56912345678",
                    Direccion = "Dirección usuario ajeno",
                    EsPlantilla = false
                };

                var presupuestoAjeno = new Presupuesto
                {
                    Id = presupuestoAjenoId,
                    UsuarioId = otroUsuarioId,
                    ClienteId = clienteAjenoId,
                    NombreProyecto = $"Presupuesto Ajeno {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                var cotizacionAjena = new Cotizacion
                {
                    Id = cotizacionAjenaId,
                    PresupuestoId = presupuestoAjenoId,
                    NumeroCotizacion = numeroCotizacion,
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                db.Usuarios.Add(otroUsuario);
                db.Clientes.Add(clienteAjeno);
                db.Presupuestos.Add(presupuestoAjeno);
                db.Cotizaciones.Add(cotizacionAjena);

                await db.SaveChangesAsync();
            }

            try
            {
                // Control: _client está autenticado como TestAuthHandler.TestUsuarioId,
                // NO como otroUsuarioId.
                usuarioAutenticadoId.Should().NotBe(otroUsuarioId);

                // Act
                var response = await _client.GetAsync(
                    $"/api/cotizaciones/{cotizacionAjenaId}/pdf");

                var responseContent = await response.Content.ReadAsByteArrayAsync();

                // Assert HTTP
                response.StatusCode.Should().NotBe(
                    HttpStatusCode.OK,
                    "un usuario no debe poder descargar el PDF de una cotización perteneciente a otro usuario");

                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.NotFound);

                // No debe haberse entregado un PDF
                if (response.Content.Headers.ContentType != null)
                {
                    response.Content.Headers.ContentType.MediaType
                        .Should().NotBe("application/pdf");
                }

                // Assert persistencia
                await using var verificationScope =
                    _factory.Services.CreateAsyncScope();

                var verificationDb = verificationScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionPersistida = await verificationDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleAsync(c => c.Id == cotizacionAjenaId);

                var presupuestoPersistido = await verificationDb.Presupuestos
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleAsync(p => p.Id == presupuestoAjenoId);

                // La solicitud ajena no debe provocar efectos secundarios
                cotizacionPersistida.Estado
                    .Should().Be("Borrador");

                cotizacionPersistida.ArchivoPdfUrl
                    .Should().BeEmpty();

                presupuestoPersistido.Estado
                    .Should().Be("Borrador");

                archivoPdfUrl = cotizacionPersistida.ArchivoPdfUrl;
            }
            finally
            {
                // Si existiera una vulnerabilidad y el PDF alcanzara a generarse,
                // también eliminamos el archivo durante el cleanup.
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionPersistida = await cleanupDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.Id == cotizacionAjenaId);

                if (cotizacionPersistida != null &&
                    !string.IsNullOrWhiteSpace(cotizacionPersistida.ArchivoPdfUrl))
                {
                    archivoPdfUrl = cotizacionPersistida.ArchivoPdfUrl;
                }

                if (!string.IsNullOrWhiteSpace(archivoPdfUrl))
                {
                    var rutaArchivo = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "Storage",
                        archivoPdfUrl.Replace(
                            "/",
                            Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(rutaArchivo))
                    {
                        File.Delete(rutaArchivo);
                    }
                }

                await cleanupDb.Cotizaciones
                    .Where(c => c.Id == cotizacionAjenaId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == presupuestoAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == clienteAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Usuarios
                    .Where(u => u.Id == otroUsuarioId)
                    .ExecuteDeleteAsync();
            }
        }

        [Fact]
        public async Task EliminarCotizacion_DeOtroUsuario_DebeDenegarAccesoYNoEliminar()
        {
            // Arrange
            var usuarioAutenticadoId = Guid.Parse(TestAuthHandler.TestUsuarioId);

            var otroUsuarioId = Guid.NewGuid();
            var clienteAjenoId = Guid.NewGuid();
            var presupuestoAjenoId = Guid.NewGuid();
            var cotizacionAjenaId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var otroUsuario = new Usuario
                {
                    Id = otroUsuarioId,
                    Rut = $"7{sufijo[..7]}-K",
                    RazonSocial = "Usuario Ajeno Integración",
                    Correo = $"usuario-ajeno-delete-{sufijo}@obrasmart.cl",
                    PasswordHash = "fake-hash",
                    PorcentajeIva = 19m,
                    ValidezCotizacionDias = 15,
                    UltimoNumeroCotizacion = 1
                };

                var clienteAjeno = new Cliente
                {
                    Id = clienteAjenoId,
                    UsuarioId = otroUsuarioId,
                    Nombre = $"Cliente Ajeno Delete {sufijo}",
                    Rut = $"6{sufijo[..7]}-K",
                    Correo = $"cliente-ajeno-delete-{sufijo}@test.cl",
                    Telefono = "+56912345678",
                    Direccion = "Dirección usuario ajeno",
                    EsPlantilla = false
                };

                var presupuestoAjeno = new Presupuesto
                {
                    Id = presupuestoAjenoId,
                    UsuarioId = otroUsuarioId,
                    ClienteId = clienteAjenoId,
                    NombreProyecto = $"Presupuesto Ajeno Delete {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                var cotizacionAjena = new Cotizacion
                {
                    Id = cotizacionAjenaId,
                    PresupuestoId = presupuestoAjenoId,
                    NumeroCotizacion = $"COT-AJENA-{sufijo[..8]}",
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                db.Usuarios.Add(otroUsuario);
                db.Clientes.Add(clienteAjeno);
                db.Presupuestos.Add(presupuestoAjeno);
                db.Cotizaciones.Add(cotizacionAjena);

                await db.SaveChangesAsync();
            }

            try
            {
                // Control: el cliente HTTP está autenticado como el usuario de pruebas,
                // no como el propietario de la cotización.
                usuarioAutenticadoId.Should().NotBe(otroUsuarioId);

                // Act
                var response = await _client.DeleteAsync(
                    $"/api/cotizaciones/{cotizacionAjenaId}");

                // Assert HTTP
                response.StatusCode.Should().NotBe(
                    HttpStatusCode.OK,
                    "un usuario autenticado no debe poder eliminar una cotización perteneciente a otro usuario");

                response.StatusCode.Should().BeOneOf(
                    HttpStatusCode.BadRequest,
                    HttpStatusCode.Unauthorized,
                    HttpStatusCode.Forbidden,
                    HttpStatusCode.NotFound);

                // Assert persistencia
                await using var verificationScope =
                    _factory.Services.CreateAsyncScope();

                var verificationDb = verificationScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var cotizacionPersistida = await verificationDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.Id == cotizacionAjenaId);

                cotizacionPersistida.Should().NotBeNull(
                    "el intento de otro usuario no debe eliminar la cotización");

                cotizacionPersistida!.Estado.Should().Be("Borrador");
                cotizacionPersistida.PresupuestoId.Should().Be(presupuestoAjenoId);

                var presupuestoPersistido = await verificationDb.Presupuestos
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Id == presupuestoAjenoId);

                presupuestoPersistido.Should().NotBeNull();
                presupuestoPersistido!.UsuarioId.Should().Be(otroUsuarioId);
            }
            finally
            {
                // Cleanup
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                // Puede que la prueba haya descubierto la vulnerabilidad
                // y la cotización ya haya sido eliminada.
                await cleanupDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == cotizacionAjenaId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == presupuestoAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.Id == clienteAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Usuarios
                    .Where(u => u.Id == otroUsuarioId)
                    .ExecuteDeleteAsync();
            }
        }

        [Fact]
        public async Task ObtenerCotizaciones_DebeRetornarSoloLasDelUsuarioAutenticado()
        {
            // Arrange
            var usuarioAutenticadoId = Guid.Parse(TestAuthHandler.TestUsuarioId);

            var otroUsuarioId = Guid.NewGuid();

            var clientePropioId = Guid.NewGuid();
            var clienteAjenoId = Guid.NewGuid();

            var presupuestoPropioId = Guid.NewGuid();
            var presupuestoAjenoId = Guid.NewGuid();

            var cotizacionPropiaId = Guid.NewGuid();
            var cotizacionAjenaId = Guid.NewGuid();

            var sufijo = Guid.NewGuid().ToString("N");

            await using (var setupScope = _factory.Services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                var usuarioAutenticado = await db.Usuarios
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(u => u.Id == usuarioAutenticadoId);

                if (usuarioAutenticado == null)
                {
                    usuarioAutenticado = new Usuario
                    {
                        Id = usuarioAutenticadoId,
                        Rut = $"9{sufijo[..7]}-K",
                        RazonSocial = "Usuario Test Integración",
                        Correo = $"test-{sufijo}@obrasmart.cl",
                        PasswordHash = "fake-hash",
                        PorcentajeIva = 19m,
                        ValidezCotizacionDias = 15,
                        UltimoNumeroCotizacion = 0
                    };

                    db.Usuarios.Add(usuarioAutenticado);
                }

                var otroUsuario = new Usuario
                {
                    Id = otroUsuarioId,
                    Rut = $"7{sufijo[..7]}-K",
                    RazonSocial = "Otro Usuario Integración",
                    Correo = $"otro-{sufijo}@obrasmart.cl",
                    PasswordHash = "fake-hash",
                    PorcentajeIva = 19m,
                    ValidezCotizacionDias = 15,
                    UltimoNumeroCotizacion = 1
                };

                var clientePropio = new Cliente
                {
                    Id = clientePropioId,
                    UsuarioId = usuarioAutenticadoId,
                    Nombre = $"Cliente Propio {sufijo}",
                    Rut = $"8{sufijo[..7]}-K",
                    Correo = $"propio-{sufijo}@test.cl",
                    Telefono = "+56911111111",
                    Direccion = "Dirección propia",
                    EsPlantilla = false
                };

                var clienteAjeno = new Cliente
                {
                    Id = clienteAjenoId,
                    UsuarioId = otroUsuarioId,
                    Nombre = $"Cliente Ajeno {sufijo}",
                    Rut = $"6{sufijo[..7]}-K",
                    Correo = $"ajeno-{sufijo}@test.cl",
                    Telefono = "+56922222222",
                    Direccion = "Dirección ajena",
                    EsPlantilla = false
                };

                var presupuestoPropio = new Presupuesto
                {
                    Id = presupuestoPropioId,
                    UsuarioId = usuarioAutenticadoId,
                    ClienteId = clientePropioId,
                    NombreProyecto = $"Presupuesto Propio {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 100000m,
                    MontoIva = 19000m,
                    Total = 119000m,
                    EsPlantilla = false
                };

                var presupuestoAjeno = new Presupuesto
                {
                    Id = presupuestoAjenoId,
                    UsuarioId = otroUsuarioId,
                    ClienteId = clienteAjenoId,
                    NombreProyecto = $"Presupuesto Ajeno {sufijo}",
                    FechaCreacion = DateTime.UtcNow,
                    Estado = "Borrador",
                    Subtotal = 200000m,
                    MontoIva = 38000m,
                    Total = 238000m,
                    EsPlantilla = false
                };

                var cotizacionPropia = new Cotizacion
                {
                    Id = cotizacionPropiaId,
                    PresupuestoId = presupuestoPropioId,
                    NumeroCotizacion = $"COT-PROP-{sufijo[..8]}",
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                var cotizacionAjena = new Cotizacion
                {
                    Id = cotizacionAjenaId,
                    PresupuestoId = presupuestoAjenoId,
                    NumeroCotizacion = $"COT-AJENA-{sufijo[..8]}",
                    FechaEmision = DateTime.UtcNow,
                    FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                    Estado = "Borrador",
                    ArchivoPdfUrl = string.Empty
                };

                db.Usuarios.Add(otroUsuario);
                db.Clientes.AddRange(clientePropio, clienteAjeno);
                db.Presupuestos.AddRange(presupuestoPropio, presupuestoAjeno);
                db.Cotizaciones.AddRange(cotizacionPropia, cotizacionAjena);

                await db.SaveChangesAsync();
            }

            try
            {
                // Act
                var response = await _client.GetAsync("/api/cotizaciones");

                var content = await response.Content.ReadAsStringAsync();

                // Assert HTTP
                response.StatusCode.Should().Be(
                    HttpStatusCode.OK,
                    $"la API respondió: {content}");

                using var json = JsonDocument.Parse(content);

                var cotizaciones = json.RootElement
                    .EnumerateArray()
                    .ToList();

                // Debe aparecer la cotización propia
                cotizaciones.Should().ContainSingle(c =>
                    c.GetProperty("id").GetGuid() == cotizacionPropiaId);

                // No debe aparecer la cotización del otro usuario
                cotizaciones.Should().NotContain(c =>
                    c.GetProperty("id").GetGuid() == cotizacionAjenaId);
            }
            finally
            {
                // Cleanup
                await using var cleanupScope =
                    _factory.Services.CreateAsyncScope();

                var cleanupDb = cleanupScope.ServiceProvider
                    .GetRequiredService<ObraSmartDbContext>();

                await cleanupDb.Cotizaciones
                    .IgnoreQueryFilters()
                    .Where(c =>
                        c.Id == cotizacionPropiaId ||
                        c.Id == cotizacionAjenaId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Presupuestos
                    .IgnoreQueryFilters()
                    .Where(p =>
                        p.Id == presupuestoPropioId ||
                        p.Id == presupuestoAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Clientes
                    .IgnoreQueryFilters()
                    .Where(c =>
                        c.Id == clientePropioId ||
                        c.Id == clienteAjenoId)
                    .ExecuteDeleteAsync();

                await cleanupDb.Usuarios
                    .Where(u => u.Id == otroUsuarioId)
                    .ExecuteDeleteAsync();
            }
        }
    }
}