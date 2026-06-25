using Microsoft.Extensions.Options;
using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs;
using ObraSmart.Application.Interfaces;
using ObraSmart.Application.Settings;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Domain.Interfaces.Services;

namespace ObraSmart.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly ConfiguracionNegocio _configuracionNegocio;

        public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService, IOptions<ConfiguracionNegocio> configuracionNegocio)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
            _configuracionNegocio = configuracionNegocio.Value;
        }

        public async Task<Result> RegistrarAsync(RegistroUsuarioDto dto)
        {
            if (await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo) != null)
                return Result.Failure("El correo ya está registrado.", "EMAIL_DUPLICADO");

            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Correo = dto.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RazonSocial = dto.RazonSocial,
                Rut = dto.Rut,
                PorcentajeIva = _configuracionNegocio.PorcentajeIvaDefecto,
                ValidezCotizacionDias = _configuracionNegocio.ValidezCotizacionDiasDefecto
            };

            await _usuarioRepository.AgregarAsync(nuevoUsuario);
            await _usuarioRepository.GuardarCambiosAsync();

            return Result.Success();
        }

        public async Task<Result<string>> LoginAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(dto.Correo);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                return Result<string>.Failure("Credenciales inválidas.", "CREDENTIALS_INVALID");

            var token = _tokenService.GenerarToken(usuario.Id, usuario.Correo);

            return Result<string>.Success(token);
        }
    }
}
