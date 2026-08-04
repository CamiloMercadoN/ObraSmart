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
                EtiquetasIds = new List<Guid>(), // Inicializamos para evitar NullReferenceException
                Componentes = new List<ComponenteAPUInputDto>
                {
                    new ComponenteAPUInputDto { InsumoId = insumo1Id, Cantidad = 2 },
                    new ComponenteAPUInputDto { InsumoId = insumo2Id, Cantidad = 1.5m }
                }
            };

            // Simulamos la respuesta de la base de datos al buscar los insumos como lista
            var insumosSimulados = new List<Insumo>
            {
                new Insumo { Id = insumo1Id, PrecioReferencia = 1000m },
                new Insumo { Id = insumo2Id, PrecioReferencia = 2000m }
            };

            _insumoRepositoryMock
                .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
                .ReturnsAsync(insumosSimulados);

            // Configuramos el repositorio de APU para capturar la entidad que se intenta guardar
            EstructuraAPU? apuGuardada = null;
            _apuRepositoryMock.Setup(r => r.AddAsync(It.IsAny<EstructuraAPU>()))
                .Callback<EstructuraAPU>(apu =>
                {
                    // Simulamos el comportamiento de Entity Framework generando el Id
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

            // Validamos la regla de negocio crítica: El cálculo matemático
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
    }
}
