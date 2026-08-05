using FluentAssertions;
using Moq;
using ObraSmart.Application.DTOs.Insumos;
using ObraSmart.Application.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class InsumoPrecioServiceTests
    {
        private readonly Mock<IInsumoRepository> _insumoRepositoryMock;
        private readonly InsumoPrecioService _sut;

        public InsumoPrecioServiceTests()
        {
            _insumoRepositoryMock = new Mock<IInsumoRepository>();

            _sut = new InsumoPrecioService(
                _insumoRepositoryMock.Object);
        }

        [Fact]
        public async Task ReajustarPreciosLoteAsync_ConPorcentaje_DebeActualizarSoloInsumosEditables()
        {
            // Arrange
            var insumoEditable1 = new Insumo
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                TipoInsumo = "Material",
                Descripcion = "Tubería PPR 20 mm",
                PrecioReferencia = 1000m,
                UnidadMedidaId = 1,
                EsPlantilla = false,
                Etiquetas = new List<Etiqueta>()
            };

            var insumoEditable2 = new Insumo
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                TipoInsumo = "ManoObra",
                Descripcion = "Hora maestro gasfíter",
                PrecioReferencia = 799.99m,
                UnidadMedidaId = 1,
                EsPlantilla = false,
                Etiquetas = new List<Etiqueta>()
            };

            var insumoPlantilla = new Insumo
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.Empty,
                TipoInsumo = "Material",
                Descripcion = "Plantilla global protegida",
                PrecioReferencia = 5000m,
                UnidadMedidaId = 1,
                EsPlantilla = true,
                Etiquetas = new List<Etiqueta>()
            };

            IReadOnlyList<Insumo> insumos = new List<Insumo>
            {
                insumoEditable1,
                insumoEditable2,
                insumoPlantilla
            };

            var dto = new ReajusteLoteDto
            {
                TipoInsumo = null,
                EtiquetaId = null,
                EsPorcentaje = true,
                Valor = 12.5m
            };

            _insumoRepositoryMock
                .Setup(r => r.GetAllWithDependenciesAsync())
                .ReturnsAsync(insumos);

            var insumosActualizados = new List<Insumo>();

            _insumoRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Insumo>()))
                .Callback<Insumo>(insumo =>
                    insumosActualizados.Add(insumo))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ReajustarPreciosLoteAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data!.Procesados.Should().Be(2);
            result.Data.Actualizados.Should().Be(2);
            result.Data.DetalleErrores.Should().BeEmpty();

            /*
             * Reajuste de 12,5 %:
             *
             * 1.000,00 × 1,125 = 1.125,00
             *   799,99 × 1,125 =   899,98875
             * Redondeado           =   899,99
             */

            insumoEditable1.PrecioReferencia.Should().Be(1125m);
            insumoEditable2.PrecioReferencia.Should().Be(899.99m);

            // Una plantilla global no debe ser modificada.
            insumoPlantilla.PrecioReferencia.Should().Be(5000m);

            insumosActualizados.Should().HaveCount(2);

            insumosActualizados.Should().Contain(
                insumo => insumo.Id == insumoEditable1.Id);

            insumosActualizados.Should().Contain(
                insumo => insumo.Id == insumoEditable2.Id);

            insumosActualizados.Should().NotContain(
                insumo => insumo.Id == insumoPlantilla.Id);

            _insumoRepositoryMock.Verify(
                r => r.GetAllWithDependenciesAsync(),
                Times.Once);

            _insumoRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Insumo>(
                    insumo =>
                        insumo.Id == insumoEditable1.Id &&
                        insumo.PrecioReferencia == 1125m)),
                Times.Once);

            _insumoRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Insumo>(
                    insumo =>
                        insumo.Id == insumoEditable2.Id &&
                        insumo.PrecioReferencia == 899.99m)),
                Times.Once);

            _insumoRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<Insumo>(
                    insumo => insumo.Id == insumoPlantilla.Id)),
                Times.Never);

            _insumoRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Insumo>()),
                Times.Exactly(2));
        }
    }
}