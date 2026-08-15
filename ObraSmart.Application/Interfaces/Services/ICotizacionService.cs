using ObraSmart.Application.Common;
using ObraSmart.Application.DTOs.Cotizaciones;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface ICotizacionService
    {
        Task<Result<IEnumerable<Cotizacion>>> ObtenerTodasPorUsuarioAsync();
        Task<Result<Cotizacion>> CrearCotizacionAsync(CrearCotizacionRequestDto request);
        Task<Result<Cotizacion>> ObtenerCotizacionPorIdAsync(Guid id);
        Task<Result<Cotizacion>> ActualizarEstadoAsync(Guid id, ActualizarEstadoCotizacionRequestDto request);
        Task<Result<Cotizacion>> RenovarVigenciaAsync(Guid id, RenovarVigenciaCotizacionRequestDto request);
        Task<Result<byte[]>> ExportarPdfAsync(Guid id);
    }
}
