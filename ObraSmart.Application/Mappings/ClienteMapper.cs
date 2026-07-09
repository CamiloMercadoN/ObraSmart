using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Mappings
{
    public static class ClienteMapper
    {
        // Mapeo de Entidad a Response DTO
        public static ClienteResponseDto ToDto(this Cliente cliente)
        {
            if (cliente == null) return null!;

            return new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Rut = cliente.Rut,
                Correo = cliente.Correo,
                Telefono = cliente.Telefono,
                Direccion = cliente.Direccion,
                CiudadId = cliente.CiudadId,
                CiudadNombre = cliente.Ciudad?.Nombre ?? string.Empty,
                EstadoProvinciaNombre = cliente.Ciudad?.EstadoProvincia?.Nombre ?? string.Empty,
                RegionId = cliente.Ciudad?.EstadoProvinciaId
            };
        }

        // Mapeo de Request DTO a Entidad (Creación)
        public static Cliente ToEntity(this ClienteRequestDto dto, Guid usuarioId)
        {
            if (dto == null) return null!;

            return new Cliente
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Nombre = dto.Nombre,
                Rut = dto.Rut,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                CiudadId = dto.CiudadId
            };
        }

        // Actualización de una entidad existente desde un DTO
        public static void UpdateEntity(this ClienteRequestDto dto, Cliente cliente)
        {
            if (dto == null || cliente == null) return;

            cliente.Nombre = dto.Nombre;
            cliente.Rut = dto.Rut;
            cliente.Correo = dto.Correo;
            cliente.Telefono = dto.Telefono;
            cliente.Direccion = dto.Direccion;
            cliente.CiudadId = dto.CiudadId;
        }
    }
}
