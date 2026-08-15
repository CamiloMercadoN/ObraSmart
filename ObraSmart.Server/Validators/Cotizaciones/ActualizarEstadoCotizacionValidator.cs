using FluentValidation;
using ObraSmart.Application.DTOs.Cotizaciones;

namespace ObraSmart.Server.Validators.Cotizaciones
{
    public class ActualizarEstadoCotizacionValidator : AbstractValidator<ActualizarEstadoCotizacionRequestDto>
    {
        public ActualizarEstadoCotizacionValidator()
        {
            RuleFor(x => x.NuevoEstado).NotEmpty().WithMessage("El estado es obligatorio.");
        }
    }
}
