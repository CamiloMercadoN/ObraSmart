using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Domain.Entities;
using ObraSmart.Domain.Interfaces.Repositories;

namespace ObraSmart.Application.Services
{
    public class CotizacionService(
            ICotizacionRepository _cotizacionRepository,
            IPresupuestoRepository _presupuestoRepository,
            IUsuarioRepository _usuarioRepository,
            ICurrentUserService _currentUserService,
            IPdfGeneratorService _pdfGeneratorService) : ICotizacionService
    {

        public async Task<Result<IEnumerable<Cotizacion>>> ObtenerTodasPorUsuarioAsync()
        {
            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null) return Result<IEnumerable<Cotizacion>>.Failure("No autenticado", "UNAUTHORIZED");

            var cotizaciones = await _cotizacionRepository.GetAllWithDependenciesByUsuarioAsync(usuarioId.Value);

            return Result<IEnumerable<Cotizacion>>.Success(cotizaciones);
        }

        public async Task<Result<Cotizacion>> CrearCotizacionAsync(CrearCotizacionRequestDto request)
        {
            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null)
            {
                return Result<Cotizacion>.Failure("Usuario no autenticado.", "UNAUTHORIZED");
            }

            // Validar Presupuesto
            var presupuesto = await _presupuestoRepository.GetByIdAsync(request.PresupuestoId);
            if (presupuesto == null || presupuesto.UsuarioId != usuarioId.Value)
            {
                return Result<Cotizacion>.Failure("Presupuesto no encontrado o no autorizado.", "NOT_FOUND");
            }

            if (presupuesto.ClienteId == null)
            {
                return Result<Cotizacion>.Failure("El presupuesto debe tener un cliente asignado para generar una cotización.", "INVALID_PRESUPUESTO");
            }

            // Obtener Usuario para correlativo
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
            if (usuario == null)
            {
                return Result<Cotizacion>.Failure("Usuario no encontrado.", "NOT_FOUND");
            }

            // Determinar Número de Cotización
            if (request.NumeroCotizacionPersonalizado.HasValue && request.NumeroCotizacionPersonalizado.Value > usuario.UltimoNumeroCotizacion)
            {
                usuario.UltimoNumeroCotizacion = request.NumeroCotizacionPersonalizado.Value;
            }
            else
            {
                usuario.UltimoNumeroCotizacion++;
            }

            var numeroGenerado = $"COT-{usuario.UltimoNumeroCotizacion}";

            // Crear Entidad
            var nuevaCotizacion = new Cotizacion
            {
                Id = Guid.NewGuid(),
                PresupuestoId = presupuesto.Id,
                NumeroCotizacion = numeroGenerado,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = request.FechaVencimiento,
                Estado = "Borrador",
                ArchivoPdfUrl = string.Empty
            };

            await _cotizacionRepository.AddAsync(nuevaCotizacion);
            await _usuarioRepository.UpdateAsync(usuario);

            return Result<Cotizacion>.Success(nuevaCotizacion);
        }

        public async Task<Result<Cotizacion>> ObtenerCotizacionPorIdAsync(Guid id)
        {
            var cotizacion = await _cotizacionRepository.GetByIdAsync(id);
            if (cotizacion == null) return Result<Cotizacion>.Failure("Cotización no encontrada.", "NOT_FOUND");

            Result<bool> validacionPropiedad = await ValidarPropiedadCotizacion(cotizacion.PresupuestoId);
            if (!validacionPropiedad.IsSuccess) return Result<Cotizacion>.Failure(validacionPropiedad.ErrorMessage ?? "", validacionPropiedad.ErrorCode ?? "");

            // Regla de Negocio: Vencimiento automático
            if (cotizacion.Estado == "Emitida" && cotizacion.FechaVencimiento.Date < DateTime.UtcNow.Date)
            {
                cotizacion.Estado = "Vencida";
                await _cotizacionRepository.UpdateAsync(cotizacion);
            }

            return Result<Cotizacion>.Success(cotizacion);
        }

        public async Task<Result<Cotizacion>> ActualizarEstadoAsync(Guid id, ActualizarEstadoCotizacionRequestDto request)
        {
            var cotizacionResult = await ObtenerCotizacionPorIdAsync(id);
            if (!cotizacionResult.IsSuccess) return cotizacionResult;

            var cotizacion = cotizacionResult.Data;

            // Regla de Negocio: Estados cerrados
            if (cotizacion.Estado == "Aceptada" || cotizacion.Estado == "Rechazada")
            {
                return Result<Cotizacion>.Failure("No se puede modificar una cotización que ya se encuentra cerrada (Aceptada o Rechazada).", "INVALID_STATE");
            }

            var estadosPermitidos = new[] { "Borrador", "Emitida", "Aceptada", "Rechazada", "Vencida" };
            if (!estadosPermitidos.Contains(request.NuevoEstado))
            {
                return Result<Cotizacion>.Failure("El estado proporcionado no es válido.", "INVALID_DATA");
            }

            cotizacion.Estado = request.NuevoEstado;
            await _cotizacionRepository.UpdateAsync(cotizacion);

            return Result<Cotizacion>.Success(cotizacion);
        }

        public async Task<Result<Cotizacion>> RenovarVigenciaAsync(Guid id, RenovarVigenciaCotizacionRequestDto request)
        {
            var cotizacionResult = await ObtenerCotizacionPorIdAsync(id);
            if (!cotizacionResult.IsSuccess) return cotizacionResult;

            var cotizacion = cotizacionResult.Data;

            if (cotizacion.Estado == "Aceptada" || cotizacion.Estado == "Rechazada")
            {
                return Result<Cotizacion>.Failure("No se puede renovar la vigencia de una cotización cerrada.", "INVALID_STATE");
            }

            cotizacion.FechaVencimiento = request.NuevaFechaVencimiento.Date;

            // Regla de Negocio: Renovar vigencia de cotización vencida
            if (cotizacion.Estado == "Vencida" && cotizacion.FechaVencimiento >= DateTime.UtcNow.Date)
            {
                cotizacion.Estado = "Emitida";
            }

            await _cotizacionRepository.UpdateAsync(cotizacion);

            return Result<Cotizacion>.Success(cotizacion);
        }

        public async Task<Result<byte[]>> ExportarPdfAsync(Guid id)
        {
            var cotizacion = await _cotizacionRepository.GetByIdWithDependenciesAsync(id);

            if (cotizacion == null)
                return Result<byte[]>.Failure("Cotización no encontrada.", "NOT_FOUND");

            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null || cotizacion.Presupuesto?.UsuarioId != usuarioId.Value)
            {
                return Result<byte[]>.Failure("No autorizado para exportar esta cotización.", "UNAUTHORIZED");
            }

            return await _pdfGeneratorService.GenerarCotizacionPdfAsync(cotizacion);
        }

        // Método auxiliar para validar propiedad (dado que el UsuarioId reside en el Presupuesto)
        private async Task<Result<bool>> ValidarPropiedadCotizacion(Guid presupuestoId)
        {
            var usuarioId = _currentUserService.GetUserId();
            if (usuarioId == null) return Result<bool>.Failure("Usuario no autenticado.", "UNAUTHORIZED");

            var presupuesto = await _presupuestoRepository.GetByIdAsync(presupuestoId);
            if (presupuesto == null || presupuesto.UsuarioId != usuarioId.Value)
            {
                return Result<bool>.Failure("No autorizado para acceder a esta cotización.", "UNAUTHORIZED");
            }

            return Result<bool>.Success(true);
        }
    }
}
