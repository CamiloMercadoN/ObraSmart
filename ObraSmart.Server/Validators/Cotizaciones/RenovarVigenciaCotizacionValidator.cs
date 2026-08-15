using FluentValidation;
using ObraSmart.Application.DTOs.Cotizaciones;

namespace ObraSmart.Server.Validators.Cotizaciones
{
    public class RenovarVigenciaCotizacionValidator : AbstractValidator<RenovarVigenciaCotizacionRequestDto>
    {
        public RenovarVigenciaCotizacionValidator()
        {
            RuleFor(x => x.NuevaFechaVencimiento)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("La nueva fecha de vencimiento no puede ser menor a la fecha actual.");
        }
    }
}
