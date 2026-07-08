using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Application.Interfaces;
using ObraSmart.Domain.Entities;
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
            var dtos = clientes.Select(c => new ClienteResponseDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Rut = c.Rut,
                Correo = c.Correo,
                Telefono = c.Telefono,
                Direccion = c.Direccion
            });

            return Result<IEnumerable<ClienteResponseDto>>.Success(dtos);
        }

        public async Task<Result<ClienteResponseDto>> ObtenerPorIdAsync(Guid id, Guid usuarioId)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id, usuarioId);

            if (cliente == null)
                return Result<ClienteResponseDto>.Failure("Cliente no encontrado.", "NOT_FOUND");

            var dto = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Rut = cliente.Rut,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion
            };

            return Result<ClienteResponseDto>.Success(dto);
        }

        public async Task<Result<ClienteResponseDto>> CrearAsync(ClienteRequestDto dto, Guid usuarioId)
        {
            var cliente = new Cliente
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Nombre = dto.Nombre,
                Rut = dto.Rut,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };

            await _clienteRepository.AgregarAsync(cliente);

            var responseDto = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Rut = cliente.Rut,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion
            };

            return Result<ClienteResponseDto>.Success(responseDto);
        }

        public async Task<Result> ActualizarAsync(Guid id, ClienteRequestDto dto, Guid usuarioId)
        {
            var cliente = await _clienteRepository.ObtenerPorIdAsync(id, usuarioId);

            if (cliente == null)
                return Result.Failure("Cliente no encontrado.", "NOT_FOUND");

            cliente.Nombre = dto.Nombre;
            cliente.Rut = dto.Rut;
            cliente.Correo = dto.Correo;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;

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
