using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.ConfiguracionComercial;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IConfiguracionComercialService
    {
        Task<Result<ConfiguracionComercialDto>> ObtenerPorUsuarioAsync(Guid usuarioId);
        Task<Result> GuardarAsync(Guid usuarioId, ConfiguracionComercialDto dto);
    }
}
