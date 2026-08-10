using FluentAssertions;
using Moq;
using ObraSmart.Application.DTOs.APUs;
using ObraSmart.Application.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class EstructuraAPUServiceTests
    {
        private readonly Mock<IEstructuraAPURepository> _apuRepositoryMock;
        private readonly Mock<IInsumoRepository> _insumoRepositoryMock;
        private readonly Mock<IRepository<Etiqueta, Guid>> _etiquetaRepositoryMock;
        private readonly EstructuraAPUService _sut; // System Under Test

        public EstructuraAPUServiceTests()
        {
            // Inicializamos los mocks con las interfaces correctas
            _apuRepositoryMock = new Mock<IEstructuraAPURepository>();
            _insumoRepositoryMock = new Mock<IInsumoRepository>();
            _etiquetaRepositoryMock = new Mock<IRepository<Etiqueta, Guid>>();

            // Inyectamos los 3 mocks al servicio
            _sut = new EstructuraAPUService(
                _apuRepositoryMock.Object,
                _insumoRepositoryMock.Object,
                _etiquetaRepositoryMock.Object);
        }

        [Fact]
        public async Task CrearAsync_ConInsumosValidos_DebeCalcularCostoTotalYRetornarSuccess()
        {
            // Arrange
            var insumo1Id = Guid.NewGuid();
            var insumo2Id = Guid.NewGuid();

            var dto = new EstructuraAPUUpsertDto
            {
                Nombre = "Instalación de Sanitario",
                UnidadMedidaId = 1,
                EtiquetasIds = new List<Guid>(),
                Componentes = new List<ComponenteAPUInputDto>
                {
                    new ComponenteAPUInputDto { InsumoId = insumo1Id, Cantidad = 2 },
                    new ComponenteAPUInputDto { InsumoId = insumo2Id, Cantidad = 1.5m }
                }
            };

            // Simula la respuesta de la base de datos al buscar los insumos como lista
            var insumosSimulados = new List<Insumo>
            {
                new Insumo { Id = insumo1Id, PrecioReferencia = 1000m },
                new Insumo { Id = insumo2Id, PrecioReferencia = 2000m }
            };

            _insumoRepositoryMock
                .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(insumosSimulados);

            // Configura el repositorio de APU para capturar la entidad que se intenta guardar
            EstructuraAPU? apuGuardada = null;
            _apuRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EstructuraAPU>()))
                .Callback<EstructuraAPU>(apu =>
                {
                    // Simula el comportamiento de Entity Framework generando el Id
                    apu.Id = Guid.NewGuid();
                    apuGuardada = apu;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CrearAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeEmpty();

            _apuRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EstructuraAPU>()), Times.Once);

            // Valida la regla de negocio crítica: El cálculo matemático
            // Costo esperado: (2 * 1000) + (1.5 * 2000) = 2000 + 3000 = 5000
            apuGuardada.Should().NotBeNull();
            apuGuardada!.CostoTotalCalculado.Should().Be(5000m);
            apuGuardada.Nombre.Should().Be(dto.Nombre);
            apuGuardada.Componentes.Should().HaveCount(2);
        }

        [Fact]
        public async Task CrearAsync_ConInsumoInexistente_DebeRetornarFailure()
        {
            // Arrange
            var insumoInvalidoId = Guid.NewGuid();

            var dto = new EstructuraAPUUpsertDto
            {
                Nombre = "Reparación Fuga",
                UnidadMedidaId = 1,
                EtiquetasIds = new List<Guid>(),
                Componentes = new List<ComponenteAPUInputDto>
                {
                    new ComponenteAPUInputDto { InsumoId = insumoInvalidoId, Cantidad = 1 }
                }
            };

            // Simulamos que el repositorio devuelve una lista vacía
            _insumoRepositoryMock
                .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(new List<Insumo>());

            // Act
            var result = await _sut.CrearAsync(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Uno o más insumos proporcionados no existen en el catálogo.");

            // Verificamos que NUNCA se llame al método AddAsync si la validación falla
            _apuRepositoryMock.Verify(r => r.AddAsync(It.IsAny<EstructuraAPU>()), Times.Never);
        }

        [Fact]
        public async Task RecalcularCostoExplicitoAsync_ConPreciosActualizados_DebeActualizarCostoTotal()
        {
            // Arrange
            var apuId = Guid.NewGuid();
            var insumo1Id = Guid.NewGuid();
            var insumo2Id = Guid.NewGuid();

            var apuExistente = new EstructuraAPU
            {
                Id = apuId,
                Nombre = "Instalación de lavaplatos",
                CostoTotalCalculado = 5000m,
                EsPlantilla = false,
                Componentes = new List<ComponenteAPU>
                {
                    new()
                    {
                        InsumoId = insumo1Id,
                        Cantidad = 2m
                    },
                    new()
                    {
                        InsumoId = insumo2Id,
                        Cantidad = 1.5m
                    }
                }
            };

            var insumosConPreciosActualizados = new List<Insumo>
            {
                new()
                {
                    Id = insumo1Id,
                    PrecioReferencia = 1250.50m
                },
                new()
                {
                    Id = insumo2Id,
                    PrecioReferencia = 2100.25m
                }
            };

            _apuRepositoryMock
                .Setup(r => r.GetByIdWithDependenciesAsync(apuId))
                .ReturnsAsync(apuExistente);

            _insumoRepositoryMock
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Count() == 2 &&
                        ids.Contains(insumo1Id) &&
                        ids.Contains(insumo2Id))))
                .ReturnsAsync(insumosConPreciosActualizados);

            EstructuraAPU? apuActualizada = null;

            _apuRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<EstructuraAPU>()))
                .Callback<EstructuraAPU>(apu => apuActualizada = apu)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RecalcularCostoExplicitoAsync(apuId);

            // Assert
            result.IsSuccess.Should().BeTrue();

            apuActualizada.Should().NotBeNull();
            apuActualizada.Should().BeSameAs(apuExistente);

            /*
             * Cálculo esperado:
             * 2 × 1.250,50 = 2.501,00
             * 1,5 × 2.100,25 = 3.150,375
             * Total sin redondear = 5.651,375
             * Total redondeado = 5.651,38
             */
            apuActualizada!.CostoTotalCalculado.Should().Be(5651.38m);

            apuActualizada.Componentes.Should().HaveCount(2);
            apuActualizada.Componentes
                .Single(c => c.InsumoId == insumo1Id)
                .Cantidad.Should().Be(2m);

            apuActualizada.Componentes
                .Single(c => c.InsumoId == insumo2Id)
                .Cantidad.Should().Be(1.5m);

            _apuRepositoryMock.Verify(
                r => r.GetByIdWithDependenciesAsync(apuId),
                Times.Once);

            _insumoRepositoryMock.Verify(
                r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
                Times.Once);

            _apuRepositoryMock.Verify(
                r => r.UpdateAsync(It.Is<EstructuraAPU>(
                    apu => apu.Id == apuId &&
                           apu.CostoTotalCalculado == 5651.38m)),
                Times.Once);
        }

        [Fact]
        public async Task ActualizarAsync_ConComponentesModificados_DebeSincronizarComponentesYRecalcularCostoTotal()
        {
            // Arrange
            var apuId = Guid.NewGuid();

            var insumoConservadoId = Guid.NewGuid();
            var insumoEliminadoId = Guid.NewGuid();
            var insumoNuevoId = Guid.NewGuid();

            var componenteConservado = new ComponenteAPU
            {
                Id = Guid.NewGuid(),
                EstructuraAPUId = apuId,
                InsumoId = insumoConservadoId,
                Cantidad = 1m
            };

            var componenteEliminado = new ComponenteAPU
            {
                Id = Guid.NewGuid(),
                EstructuraAPUId = apuId,
                InsumoId = insumoEliminadoId,
                Cantidad = 2m
            };

            var apuExistente = new EstructuraAPU
            {
                Id = apuId,
                UsuarioId = Guid.NewGuid(),
                Nombre = "APU antes de actualizar",
                UnidadMedidaId = 1,
                CostoTotalCalculado = 5000m,
                EsPlantilla = false,
                Componentes = new List<ComponenteAPU>
                {
                    componenteConservado,
                    componenteEliminado
                },
                Etiquetas = new List<Etiqueta>()
            };

            var dto = new EstructuraAPUUpsertDto
            {
                Nombre = "APU actualizada",
                UnidadMedidaId = 2,
                EtiquetasIds = new List<Guid>(),
                Componentes = new List<ComponenteAPUInputDto>
                {
                    // Este componente ya existía: debe actualizarse su cantidad.
                    new()
                    {
                        InsumoId = insumoConservadoId,
                        Cantidad = 3m
                    },

                    // Este componente no existía: debe agregarse.
                    new()
                    {
                        InsumoId = insumoNuevoId,
                        Cantidad = 1.5m
                    }
                }
            };

            var insumosActuales = new List<Insumo>
            {
                new()
                {
                    Id = insumoConservadoId,
                    PrecioReferencia = 1250.50m
                },
                new()
                {
                    Id = insumoNuevoId,
                    PrecioReferencia = 2100.25m
                }
            };

            _apuRepositoryMock
                .Setup(r => r.GetByIdWithDependenciesAsync(apuId))
                .ReturnsAsync(apuExistente);

            _insumoRepositoryMock
                .Setup(r => r.GetByIdsAsync(
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Count() == 2 &&
                        ids.Contains(insumoConservadoId) &&
                        ids.Contains(insumoNuevoId))))
                .ReturnsAsync(insumosActuales);

            EstructuraAPU? apuActualizada = null;
            List<ComponenteAPU>? componentesEliminados = null;

            _apuRepositoryMock
                .Setup(r => r.UpdateGrafoAsync(
                    It.IsAny<EstructuraAPU>(),
                    It.IsAny<IEnumerable<ComponenteAPU>>()))
                .Callback<EstructuraAPU, IEnumerable<ComponenteAPU>>(
                    (apu, eliminados) =>
                    {
                        apuActualizada = apu;
                        componentesEliminados = eliminados.ToList();
                    })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ActualizarAsync(apuId, dto);

            // Assert
            result.IsSuccess.Should().BeTrue();

            apuActualizada.Should().NotBeNull();
            apuActualizada.Should().BeSameAs(apuExistente);

            apuActualizada!.Nombre.Should().Be("APU actualizada");
            apuActualizada.UnidadMedidaId.Should().Be(2);

            /*
             * Cálculo:
             *
             * Componente conservado:
             * 3 × 1.250,50 = 3.751,50
             *
             * Componente nuevo:
             * 1,5 × 2.100,25 = 3.150,375
             *
             * Total sin redondear = 6.901,875
             * Total redondeado    = 6.901,88
             */
            apuActualizada.CostoTotalCalculado.Should().Be(6901.88m);

            // Deben quedar solamente el componente conservado y el nuevo.
            apuActualizada.Componentes.Should().HaveCount(2);

            apuActualizada.Componentes.Should().ContainSingle(
                componente =>
                    componente.InsumoId == insumoConservadoId &&
                    componente.Cantidad == 3m);

            apuActualizada.Componentes.Should().ContainSingle(
                componente =>
                    componente.InsumoId == insumoNuevoId &&
                    componente.Cantidad == 1.5m);

            apuActualizada.Componentes.Should().NotContain(
                componente =>
                    componente.InsumoId == insumoEliminadoId);

            // El componente retirado debe enviarse al repositorio para su eliminación.
            componentesEliminados.Should().NotBeNull();
            componentesEliminados.Should().ContainSingle();

            componentesEliminados!.Single()
                .Should().BeSameAs(componenteEliminado);

            _apuRepositoryMock.Verify(
                r => r.GetByIdWithDependenciesAsync(apuId),
                Times.Once);

            _insumoRepositoryMock.Verify(
                r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
                Times.Once);

            _apuRepositoryMock.Verify(
                r => r.UpdateGrafoAsync(
                    It.Is<EstructuraAPU>(apu =>
                        apu.Id == apuId &&
                        apu.Nombre == dto.Nombre &&
                        apu.UnidadMedidaId == dto.UnidadMedidaId &&
                        apu.CostoTotalCalculado == 6901.88m &&
                        apu.Componentes.Count == 2),
                    It.Is<IEnumerable<ComponenteAPU>>(eliminados =>
                        eliminados.Count() == 1 &&
                        eliminados.Single().Id == componenteEliminado.Id)),
                Times.Once);

            _apuRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<EstructuraAPU>()),
                Times.Never);
        }
    }
}
