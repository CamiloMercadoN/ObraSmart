using FluentAssertions;
using Moq;
using ObraSmart.Application.DTOs.Presupuestos;
using ObraSmart.Application.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class PresupuestoServiceTests
    {
        private readonly Mock<IPresupuestoRepository> _presupuestoRepositoryMock;
        private readonly Mock<IRepository<Usuario, Guid>> _usuarioRepositoryMock;
        private readonly PresupuestoService _sut;

        public PresupuestoServiceTests()
        {
            _presupuestoRepositoryMock = new Mock<IPresupuestoRepository>();
            _usuarioRepositoryMock = new Mock<IRepository<Usuario, Guid>>();

            _sut = new PresupuestoService(
                _presupuestoRepositoryMock.Object,
                _usuarioRepositoryMock.Object);
        }

        [Fact]
        public async Task CrearAsync_ConRecursosValidos_DebeCalcularSubtotalIvaYTotal()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();

            var usuario = new Usuario
            {
                Id = usuarioId,
                PorcentajeIva = 19m
            };

            var dto = new PresupuestoUpsertDto
            {
                NombreProyecto = "Instalación sanitaria vivienda",
                Items = new List<ItemPresupuestoUpsertDto>
                {
                    new()
                    {
                        Descripcion = "Instalación de lavaplatos",
                        CantidadItem = 2m,
                        UnidadMedidaId = 1,
                        Recursos = new List<RecursoItemPresupuestoUpsertDto>
                        {
                            new()
                            {
                                TipoInsumo = "Material",
                                DescripcionCongelada = "Tubería PPR 20 mm",
                                Cantidad = 3m,
                                PrecioUnitarioCongelado = 1000.50m,
                                UnidadMedidaId = 1
                            },
                            new()
                            {
                                TipoInsumo = "ManoObra",
                                DescripcionCongelada = "Maestro gasfíter",
                                Cantidad = 1.5m,
                                PrecioUnitarioCongelado = 2000.25m,
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
                                DescripcionCongelada = "Equipo de prueba",
                                Cantidad = 2m,
                                PrecioUnitarioCongelado = 500.10m,
                                UnidadMedidaId = 1
                            }
                        }
                    }
                }
            };

            _usuarioRepositoryMock
                .Setup(r => r.GetByIdAsync(usuarioId))
                .ReturnsAsync(usuario);

            Presupuesto? presupuestoGuardado = null;

            _presupuestoRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Presupuesto>()))
                .Callback<Presupuesto>(presupuesto =>
                {
                    presupuesto.Id = Guid.NewGuid();
                    presupuestoGuardado = presupuesto;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CrearAsync(dto, usuarioId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeEmpty();

            presupuestoGuardado.Should().NotBeNull();
            presupuestoGuardado!.UsuarioId.Should().Be(usuarioId);
            presupuestoGuardado.NombreProyecto.Should().Be(dto.NombreProyecto);
            presupuestoGuardado.Estado.Should().Be("Borrador");
            presupuestoGuardado.EsPlantilla.Should().BeFalse();
            presupuestoGuardado.Items.Should().HaveCount(2);

            /*
             * Ítem 1:
             * Recurso 1: 3 × 1.000,50 = 3.001,50
             * Recurso 2: 1,5 × 2.000,25 = 3.000,375
             *             Redondeado     = 3.000,38
             *
             * Precio unitario ítem 1 = 6.001,88
             * Subtotal ítem 1 = 2 × 6.001,88 = 12.003,76
             *
             * Ítem 2:
             * Recurso: 2 × 500,10 = 1.000,20
             * Subtotal ítem 2 = 1,5 × 1.000,20 = 1.500,30
             *
             * Subtotal presupuesto = 13.504,06
             * IVA 19 %              =  2.565,77
             * Total                 = 16.069,83
             */

            presupuestoGuardado.Subtotal.Should().Be(13504.06m);
            presupuestoGuardado.MontoIva.Should().Be(2565.77m);
            presupuestoGuardado.Total.Should().Be(16069.83m);

            var primerItem = presupuestoGuardado.Items
                .Single(i => i.Descripcion == "Instalación de lavaplatos");

            primerItem.PrecioUnitarioCalculado.Should().Be(6001.88m);
            primerItem.Subtotal.Should().Be(12003.76m);
            primerItem.Recursos.Should().HaveCount(2);

            var recursoMaterial = primerItem.Recursos
                .Single(r => r.DescripcionCongelada == "Tubería PPR 20 mm");

            recursoMaterial.PrecioUnitarioCongelado.Should().Be(1000.50m);
            recursoMaterial.CostoTotalRecurso.Should().Be(3001.50m);

            var recursoManoObra = primerItem.Recursos
                .Single(r => r.DescripcionCongelada == "Maestro gasfíter");

            recursoManoObra.PrecioUnitarioCongelado.Should().Be(2000.25m);
            recursoManoObra.CostoTotalRecurso.Should().Be(3000.38m);

            var segundoItem = presupuestoGuardado.Items
                .Single(i => i.Descripcion == "Prueba de funcionamiento");

            segundoItem.PrecioUnitarioCalculado.Should().Be(1000.20m);
            segundoItem.Subtotal.Should().Be(1500.30m);

            _usuarioRepositoryMock.Verify(
                r => r.GetByIdAsync(usuarioId),
                Times.Once);

            _presupuestoRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Presupuesto>(p =>
                    p.UsuarioId == usuarioId &&
                    p.Subtotal == 13504.06m &&
                    p.MontoIva == 2565.77m &&
                    p.Total == 16069.83m)),
                Times.Once);
        }

        [Fact]
        public async Task ActualizarAsync_ConPresupuestoNoBorrador_DebeRetornarFailureYNoActualizar()
        {
            // Arrange
            var presupuestoId = Guid.NewGuid();
            var usuarioId = Guid.NewGuid();

            var presupuestoExistente = new Presupuesto
            {
                Id = presupuestoId,
                UsuarioId = usuarioId,
                NombreProyecto = "Instalación sanitaria emitida",
                Estado = "Emitido",
                Subtotal = 10000m,
                MontoIva = 1900m,
                Total = 11900m,
                EsPlantilla = false
            };

            var dto = new PresupuestoUpsertDto
            {
                NombreProyecto = "Nombre que no debe aplicarse",
                Items = new List<ItemPresupuestoUpsertDto>()
            };

            _presupuestoRepositoryMock
                .Setup(r => r.GetByIdWithDependenciesAsync(presupuestoId))
                .ReturnsAsync(presupuestoExistente);

            // Act
            var result = await _sut.ActualizarAsync(
                presupuestoId,
                dto,
                usuarioId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("INVALID_STATE");
            result.ErrorMessage.Should()
                .Be("Solo se pueden editar presupuestos en estado Borrador.");

            // La entidad debe permanecer sin modificaciones.
            presupuestoExistente.NombreProyecto.Should()
                .Be("Instalación sanitaria emitida");

            presupuestoExistente.Subtotal.Should().Be(10000m);
            presupuestoExistente.MontoIva.Should().Be(1900m);
            presupuestoExistente.Total.Should().Be(11900m);
            presupuestoExistente.Estado.Should().Be("Emitido");

            _presupuestoRepositoryMock.Verify(
                r => r.GetByIdWithDependenciesAsync(presupuestoId),
                Times.Once);

            // El rechazo ocurre antes de buscar la configuración del usuario.
            _usuarioRepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<Guid>()),
                Times.Never);

            // No debe ejecutarse ninguna actualización compuesta.
            _presupuestoRepositoryMock.Verify(
                r => r.UpdateGrafoAsync(
                    It.IsAny<Presupuesto>(),
                    It.IsAny<IEnumerable<ItemPresupuesto>>(),
                    It.IsAny<IEnumerable<RecursoItemPresupuesto>>()),
                Times.Never);

            _presupuestoRepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Presupuesto>()),
                Times.Never);
        }
    }
}