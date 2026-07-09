using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Application.Mappings;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Result<IEnumerable<ClienteResponseDto>>> ObtenerTodosAsync()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            var dtos = clientes.Select(c => c.ToDto());

            return Result<IEnumerable<ClienteResponseDto>>.Success(dtos);
        }

        public async Task<Result<ClienteResponseDto>> ObtenerPorIdAsync(Guid id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
                return Result<ClienteResponseDto>.Failure("Cliente no encontrado.", "NOT_FOUND");

            return Result<ClienteResponseDto>.Success(cliente.ToDto());
        }

        public async Task<Result<ClienteResponseDto>> CrearAsync(ClienteRequestDto dto, Guid usuarioId)
        {
            var cliente = dto.ToEntity(usuarioId);

            await _clienteRepository.AddAsync(cliente);

            return Result<ClienteResponseDto>.Success(cliente.ToDto());
        }

        public async Task<Result> ActualizarAsync(Guid id, ClienteRequestDto dto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
                return Result.Failure("Cliente no encontrado.", "NOT_FOUND");

            // Mapeo inverso de actualización
            dto.UpdateEntity(cliente);

            await _clienteRepository.UpdateAsync(cliente);

            return Result.Success();
        }

        public async Task<Result> EliminarAsync(Guid id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
                return Result.Failure("Cliente no encontrado.", "NOT_FOUND");

            bool tienePresupuestos = await _clienteRepository.TienePresupuestosAsociadosAsync(id);
            if (tienePresupuestos)
                return Result.Failure("No se puede eliminar el cliente porque tiene presupuestos asociados.", "CONFLICT");

            await _clienteRepository.DeleteAsync(cliente);

            return Result.Success();
        }
    }
}
