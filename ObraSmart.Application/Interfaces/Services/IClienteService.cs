using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Clientes;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IClienteService
    {
        Task<Result<IEnumerable<ClienteResponseDto>>> ObtenerTodosAsync();
        Task<Result<ClienteResponseDto>> ObtenerPorIdAsync(Guid id);
        Task<Result<ClienteResponseDto>> CrearAsync(ClienteRequestDto dto);
        Task<Result> ActualizarAsync(Guid id, ClienteRequestDto dto);
        Task<Result> EliminarAsync(Guid id);
    }
}
