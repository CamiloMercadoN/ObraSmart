using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.ConfiguracionComercial;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class ConfiguracionComercialService(
            IUsuarioRepository usuarioRepository,
            IFileService fileService) : IConfiguracionComercialService
    {
        public async Task<Result<ConfiguracionComercialDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
        {
            var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null)
            {
                return Result<ConfiguracionComercialDto>.Failure("Usuario no encontrado.", "USER_NOT_FOUND");
            }

            return Result<ConfiguracionComercialDto>.Success(usuario.ToDto());
        }

        public async Task<Result> GuardarAsync(Guid usuarioId, ConfiguracionComercialDto dto)
        {
            var usuario = await usuarioRepository.GetByIdAsync(usuarioId);

            if (usuario == null)
            {
                return Result.Failure("Usuario no encontrado.", "USER_NOT_FOUND");
            }

            string? nuevaRutaLogo = null;

            // Identificamos si el front envió un Base64 nuevo o si es la misma URL estática
            if (!string.IsNullOrWhiteSpace(dto.LogoBase64) && dto.LogoBase64.StartsWith("data:image"))
            {
                // Limpiar la imagen anterior del servidor si ya tenía una
                if (!string.IsNullOrWhiteSpace(usuario.LogoUrl))
                {
                    fileService.EliminarArchivo(usuario.LogoUrl);
                }

                // Generar y guardar el nuevo archivo
                nuevaRutaLogo = await fileService.GuardarImagenBase64Async(
                    dto.LogoBase64,
                    "uploads/logos",
                    $"logo-{usuarioId}");
            }

            // Aplicamos los cambios a la entidad
            usuario.UpdateEntity(dto, nuevaRutaLogo);

            await usuarioRepository.UpdateAsync(usuario);

            return Result.Success();
        }
    }
}
