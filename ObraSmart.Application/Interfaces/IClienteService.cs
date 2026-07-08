using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Clientes;

namespace ObraSmart.Application.Interfaces
{
    public interface IClienteService
    {
        Task<Result<IEnumerable<ClienteResponseDto>>> ObtenerTodosAsync(Guid usuarioId);
        Task<Result<ClienteResponseDto>> ObtenerPorIdAsync(Guid id, Guid usuarioId);
        Task<Result<ClienteResponseDto>> CrearAsync(ClienteRequestDto dto, Guid usuarioId);
        Task<Result> ActualizarAsync(Guid id, ClienteRequestDto dto, Guid usuarioId);
        Task<Result> EliminarAsync(Guid id, Guid usuarioId);
    }
}
