using ObraSmart.Application.Common;
using ObraSmart.Domain.Entities;

namespace ObraSmart.Application.Interfaces.Services
{
    public interface IPdfGeneratorService
    {
        Task<Result<byte[]>> GenerarCotizacionPdfAsync(Cotizacion cotizacion, bool incluirRecursos);
    }
}
