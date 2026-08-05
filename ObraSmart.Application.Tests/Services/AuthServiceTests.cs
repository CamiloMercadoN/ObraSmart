using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ObraSmart.Application.DTOs;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Services;
using ObraSmart.Application.Settings;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPasswordHasherService> _passwordHasherMock;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _passwordHasherMock = new Mock<IPasswordHasherService>();

            var configuracionNegocio = Options.Create(
                new ConfiguracionNegocio
                {
                    PorcentajeIvaDefecto = 19m,
                    ValidezCotizacionDiasDefecto = 15
                });

            _sut = new AuthService(
                _usuarioRepositoryMock.Object,
                _tokenServiceMock.Object,
                _passwordHasherMock.Object,
                configuracionNegocio);
        }

        [Fact]
        public async Task RegistrarAsync_ConDatosValidos_DebeGuardarPasswordHasheadaYValoresPorDefecto()
        {
            // Arrange
            var dto = new RegistroUsuarioDto
            {
                Correo = "maestro.gasfiter@obrasmart.cl",
                Password = "ClaveSegura123!",
                RazonSocial = "Servicios Sanitarios ObraSmart",
                Rut = "12.345.678-5"
            };

            const string passwordHashEsperada =
                "$2a$11$hash-generada-para-prueba-unitaria";

            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorCorreoAsync(dto.Correo))
                .ReturnsAsync((Usuario?)null);

            _passwordHasherMock
                .Setup(h => h.HashPassword(dto.Password))
                .Returns(passwordHashEsperada);

            Usuario? usuarioGuardado = null;

            _usuarioRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(usuario => usuarioGuardado = usuario)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.RegistrarAsync(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.ErrorCode.Should().BeNull();
            result.ErrorMessage.Should().BeNull();

            usuarioGuardado.Should().NotBeNull();

            usuarioGuardado!.Id.Should().NotBe(Guid.Empty);
            usuarioGuardado.Correo.Should().Be(dto.Correo);
            usuarioGuardado.RazonSocial.Should().Be(dto.RazonSocial);
            usuarioGuardado.Rut.Should().Be(dto.Rut);

            usuarioGuardado.PasswordHash.Should().Be(passwordHashEsperada);
            usuarioGuardado.PasswordHash.Should().NotBe(dto.Password);

            usuarioGuardado.PorcentajeIva.Should().Be(19m);
            usuarioGuardado.ValidezCotizacionDias.Should().Be(15);

            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorCorreoAsync(dto.Correo),
                Times.Once);

            _passwordHasherMock.Verify(
                h => h.HashPassword(dto.Password),
                Times.Once);

            _usuarioRepositoryMock.Verify(
                r => r.AddAsync(It.Is<Usuario>(usuario =>
                    usuario.Correo == dto.Correo &&
                    usuario.PasswordHash == passwordHashEsperada &&
                    usuario.PasswordHash != dto.Password &&
                    usuario.PorcentajeIva == 19m &&
                    usuario.ValidezCotizacionDias == 15)),
                Times.Once);

            _tokenServiceMock.Verify(
                t => t.GenerarToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task RegistrarAsync_ConCorreoDuplicado_DebeRetornarFailureYNoCrearUsuario()
        {
            // Arrange
            var dto = new RegistroUsuarioDto
            {
                Correo = "maestro.existente@obrasmart.cl",
                Password = "OtraClaveSegura123!",
                RazonSocial = "Nuevo servicio de gasfitería",
                Rut = "15.345.678-9"
            };

            var usuarioExistente = new Usuario
            {
                Id = Guid.NewGuid(),
                Correo = dto.Correo,
                PasswordHash = "$2a$11$hash-del-usuario-existente",
                RazonSocial = "Servicios existentes",
                Rut = "12.345.678-5",
                PorcentajeIva = 19m,
                ValidezCotizacionDias = 15
            };

            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorCorreoAsync(dto.Correo))
                .ReturnsAsync(usuarioExistente);

            // Act
            var result = await _sut.RegistrarAsync(dto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorCode.Should().Be("EMAIL_DUPLICADO");
            result.ErrorMessage.Should().Be("El correo ya está registrado.");

            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorCorreoAsync(dto.Correo),
                Times.Once);

            _passwordHasherMock.Verify(
                h => h.HashPassword(It.IsAny<string>()),
                Times.Never);

            _usuarioRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Usuario>()),
                Times.Never);

            _tokenServiceMock.Verify(
                t => t.GenerarToken(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }
    }
}