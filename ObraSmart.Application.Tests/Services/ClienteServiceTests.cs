using FluentAssertions;
using Moq;
using ObraSmart.Application.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly ClienteService _sut;

        public ClienteServiceTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();

            _sut = new ClienteService(
                _clienteRepositoryMock.Object);
        }

        [Fact]
        public async Task EliminarAsync_ConPresupuestosAsociados_DebeRetornarConflictYNoEliminarCliente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();

            var clienteExistente = new Cliente
            {
                Id = clienteId,
                UsuarioId = Guid.NewGuid(),
                Nombre = "Cliente con presupuesto",
                Rut = "12.345.678-5",
                Correo = "cliente@obrasmart.cl",
                Telefono = "+56912345678",
                Direccion = "Dirección de prueba",
                EsPlantilla = false
            };

            _clienteRepositoryMock
                .Setup(r => r.GetByIdAsync(clienteId))
                .ReturnsAsync(clienteExistente);

            _clienteRepositoryMock
                .Setup(r => r.TienePresupuestosAsociadosAsync(clienteId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.EliminarAsync(clienteId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("CONFLICT");
            result.ErrorMessage.Should().Be(
                "No se puede eliminar el cliente porque tiene presupuestos asociados.");

            _clienteRepositoryMock.Verify(
                r => r.GetByIdAsync(clienteId),
                Times.Once);

            _clienteRepositoryMock.Verify(
                r => r.TienePresupuestosAsociadosAsync(clienteId),
                Times.Once);

            _clienteRepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<Cliente>()),
                Times.Never);
        }
    }
}