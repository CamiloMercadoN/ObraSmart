using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Application.Interfaces;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Service
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Result<IEnumerable<ClienteResponseDto>>> ObtenerTodosAsync(Guid usuarioId)
        {
            var clientes = await _clienteRepository.ObtenerTodosAsync(usuarioId);
            var dtos = clientes.Select(c => c.ToDto());

            return Result<IEnumerable<ClienteResponseDto>>.Success(dtos);
        }

        public async Task<Result<ClienteResponseDto>> ObtenerPorIdAsync(Guid id, Guid usuarioId)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id, usuarioId);

            if (cliente == null)
                return Result<ClienteResponseDto>.Failure("Cliente no encontrado.", "NOT_FOUND");

            return Result<ClienteResponseDto>.Success(cliente.ToDto());
        }

        public async Task<Result<ClienteResponseDto>> CrearAsync(ClienteRequestDto dto, Guid usuarioId)
        {
            var cliente = dto.ToEntity(usuarioId);

            await _clienteRepository.AgregarAsync(cliente);

            return Result<ClienteResponseDto>.Success(cliente.ToDto());
        }

        public async Task<Result> ActualizarAsync(Guid id, ClienteRequestDto dto, Guid usuarioId)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id, usuarioId);

            if (cliente == null)
                return Result.Failure("Cliente no encontrado.", "NOT_FOUND");

            // Mapeo inverso de actualización
            dto.UpdateEntity(cliente);

            await _clienteRepository.ActualizarAsync(cliente);

            return Result.Success();
        }

        public async Task<Result> EliminarAsync(Guid id, Guid usuarioId)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id, usuarioId);

            if (cliente == null)
                return Result.Failure("Cliente no encontrado.", "NOT_FOUND");

            bool tienePresupuestos = await _clienteRepository.TienePresupuestosAsociadosAsync(id);
            if (tienePresupuestos)
                return Result.Failure("No se puede eliminar el cliente porque tiene presupuestos asociados.", "CONFLICT");

            await _clienteRepository.EliminarAsync(cliente);

            return Result.Success();
        }
    }
}
