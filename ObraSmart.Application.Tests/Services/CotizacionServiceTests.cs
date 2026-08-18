using FluentAssertions;
using Moq;
using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class CotizacionServiceTests
    {
        private readonly Mock<ICotizacionRepository> _cotizacionRepositoryMock;
        private readonly Mock<IPresupuestoRepository> _presupuestoRepositoryMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IPdfGeneratorService> _pdfGeneratorServiceMock;

        private readonly CotizacionService _sut;

        private readonly Guid _usuarioId = Guid.NewGuid();

        public CotizacionServiceTests()
        {
            _cotizacionRepositoryMock = new Mock<ICotizacionRepository>();
            _presupuestoRepositoryMock = new Mock<IPresupuestoRepository>();
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _pdfGeneratorServiceMock = new Mock<IPdfGeneratorService>();

            _currentUserServiceMock
                .Setup(s => s.GetUserId())
                .Returns(_usuarioId);

            _sut = new CotizacionService(
                _cotizacionRepositoryMock.Object,
                _presupuestoRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _currentUserServiceMock.Object,
                _pdfGeneratorServiceMock.Object);
        }

        [Fact]
        public async Task CrearCotizacionAsync_ConPresupuestoValido_DebeGenerarCorrelativoYEstadoBorrador()
        {
            // Arrange
            var presupuestoId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = clienteId,
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Borrador"
            };

            var usuario = new Usuario
            {
                Id = _usuarioId,
                UltimoNumeroCotizacion = 10
            };

            var request = new CrearCotizacionRequestDto
            {
                PresupuestoId = presupuestoId,
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(15)
            };

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            _usuarioRepositoryMock
                .Setup(r => r.GetByIdAsync(_usuarioId))
                .ReturnsAsync(usuario);

            _cotizacionRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Cotizacion>()))
                .Returns(Task.CompletedTask);

            _usuarioRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Usuario>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CrearCotizacionAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.NumeroCotizacion.Should().Be("COT-11");
            result.Data.Estado.Should().Be("Borrador");
            result.Data.PresupuestoId.Should().Be(presupuestoId);
            result.Data.FechaVencimiento.Should().Be(request.FechaVencimiento);

            usuario.UltimoNumeroCotizacion.Should().Be(11);

            _cotizacionRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Cotizacion>(c =>
                    c.PresupuestoId == presupuestoId &&
                    c.NumeroCotizacion == "COT-11" &&
                    c.Estado == "Borrador")),
                Times.Once);

            _usuarioRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Usuario>(u =>
                    u.Id == _usuarioId &&
                    u.UltimoNumeroCotizacion == 11)),
                Times.Once);
        }

        [Fact]
        public async Task CrearCotizacionAsync_SinCliente_DebeRetornarFailureYNoPersistir()
        {
            // Arrange
            var presupuestoId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = null,
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Borrador"
            };

            var request = new CrearCotizacionRequestDto
            {
                PresupuestoId = presupuestoId,
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(15)
            };

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            // Act
            var result = await _sut.CrearCotizacionAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_PRESUPUESTO");
            result.ErrorMessage.Should().Be(
                "El presupuesto debe tener un cliente asignado para generar una cotización.");

            _cotizacionRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Cotizacion>()),
                Times.Never);

            _usuarioRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Usuario>()),
                Times.Never);

            _usuarioRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task ActualizarEstadoAsync_AEmitida_DebeSincronizarEstadoPresupuesto()
        {
            // Arrange
            var cotizacionId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = Guid.NewGuid(),
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Borrador"
            };

            var cotizacion = new Cotizacion
            {
                Id = cotizacionId,
                PresupuestoId = presupuestoId,
                NumeroCotizacion = "COT-12",
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                Estado = "Borrador",
                ArchivoPdfUrl = string.Empty
            };

            var request = new ActualizarEstadoCotizacionRequestDto
            {
                NuevoEstado = "Emitida"
            };

            _cotizacionRepositoryMock
                .Setup(r => r.GetByIdAsync(cotizacionId))
                .ReturnsAsync(cotizacion);

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            _cotizacionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Cotizacion>()))
                .Returns(Task.CompletedTask);

            _presupuestoRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Presupuesto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ActualizarEstadoAsync(cotizacionId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.Estado.Should().Be("Emitida");
            presupuesto.Estado.Should().Be("Emitido");

            _presupuestoRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Presupuesto>(p =>
                    p.Id == presupuestoId &&
                    p.Estado == "Emitido")),
                Times.Once);

            _cotizacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Cotizacion>(c =>
                    c.Id == cotizacionId &&
                    c.Estado == "Emitida")),
                Times.Once);
        }

        [Fact]
        public async Task ActualizarEstadoAsync_ConCotizacionCerrada_DebeRetornarFailureYNoModificar()
        {
            // Arrange
            var cotizacionId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = Guid.NewGuid(),
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Aprobado"
            };

            var cotizacion = new Cotizacion
            {
                Id = cotizacionId,
                PresupuestoId = presupuestoId,
                NumeroCotizacion = "COT-13",
                FechaEmision = DateTime.UtcNow.AddDays(-2),
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(15),
                Estado = "Aceptada",
                ArchivoPdfUrl = string.Empty
            };

            var request = new ActualizarEstadoCotizacionRequestDto
            {
                NuevoEstado = "Rechazada"
            };

            _cotizacionRepositoryMock
                .Setup(r => r.GetByIdAsync(cotizacionId))
                .ReturnsAsync(cotizacion);

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            // Act
            var result = await _sut.ActualizarEstadoAsync(cotizacionId, request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_STATE");
            result.ErrorMessage.Should().Be(
                "No se puede modificar una cotización que ya se encuentra cerrada (Aceptada o Rechazada).");

            cotizacion.Estado.Should().Be("Aceptada");
            presupuesto.Estado.Should().Be("Aprobado");

            _cotizacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Cotizacion>()),
                Times.Never);

            _presupuestoRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Presupuesto>()),
                Times.Never);
        }

        [Fact]
        public async Task ObtenerCotizacionPorIdAsync_EmitidaVencida_DebeCambiarEstadoAVencida()
        {
            // Arrange
            var cotizacionId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = Guid.NewGuid(),
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Emitido"
            };

            var cotizacion = new Cotizacion
            {
                Id = cotizacionId,
                PresupuestoId = presupuestoId,
                NumeroCotizacion = "COT-14",
                FechaEmision = DateTime.UtcNow.AddDays(-10),
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(-1),
                Estado = "Emitida",
                ArchivoPdfUrl = string.Empty
            };

            _cotizacionRepositoryMock
                .Setup(r => r.GetByIdAsync(cotizacionId))
                .ReturnsAsync(cotizacion);

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            _cotizacionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Cotizacion>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ObtenerCotizacionPorIdAsync(cotizacionId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.Estado.Should().Be("Vencida");
            cotizacion.Estado.Should().Be("Vencida");

            _cotizacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Cotizacion>(c =>
                    c.Id == cotizacionId &&
                    c.Estado == "Vencida")),
                Times.Once);
        }

        [Fact]
        public async Task RenovarVigenciaAsync_ConCotizacionVencidaYFechaFutura_DebeReactivarComoEmitida()
        {
            // Arrange
            var cotizacionId = Guid.NewGuid();
            var presupuestoId = Guid.NewGuid();

            var presupuesto = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = _usuarioId,
                ClienteId = Guid.NewGuid(),
                NombreProyecto = "Instalación sanitaria vivienda",
                Estado = "Emitido"
            };

            var cotizacion = new Cotizacion
            {
                Id = cotizacionId,
                PresupuestoId = presupuestoId,
                NumeroCotizacion = "COT-15",
                FechaEmision = DateTime.UtcNow.AddDays(-20),
                FechaVencimiento = DateTime.UtcNow.Date.AddDays(-5),
                Estado = "Vencida",
                ArchivoPdfUrl = string.Empty
            };

            var nuevaFechaVencimiento = DateTime.UtcNow.Date.AddDays(15);

            var request = new RenovarVigenciaCotizacionRequestDto
            {
                NuevaFechaVencimiento = nuevaFechaVencimiento
            };

            _cotizacionRepositoryMock
                .Setup(r => r.GetByIdAsync(cotizacionId))
                .ReturnsAsync(cotizacion);

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdAsync(presupuestoId))
                .ReturnsAsync(presupuesto);

            _cotizacionRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Cotizacion>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RenovarVigenciaAsync(cotizacionId, request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.Estado.Should().Be("Emitida");
            result.Data.FechaVencimiento.Should().Be(nuevaFechaVencimiento);

            cotizacion.Estado.Should().Be("Emitida");
            cotizacion.FechaVencimiento.Should().Be(nuevaFechaVencimiento);

            _cotizacionRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Cotizacion>(c =>
                    c.Id == cotizacionId &&
                    c.Estado == "Emitida" &&
                    c.FechaVencimiento == nuevaFechaVencimiento)),
                Times.Once);
        }
    }
}