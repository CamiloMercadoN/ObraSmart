using FluentValidation;
using ObraSmart.Application.DTOs.Cotizaciones;

namespace ObraSmart.Server.Validators.Cotizaciones
{
    public class CrearCotizacionValidator : AbstractValidator<CrearCotizacionRequestDto>
    {
        public CrearCotizacionValidator()
        {
            RuleFor(x => x.PresupuestoId)
                .NotEmpty().WithMessage("El PresupuestoId es obligatorio.");

            RuleFor(x => x.FechaVencimiento)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("La fecha de vencimiento no puede ser menor a la fecha actual.");

            RuleFor(x => x.NumeroCotizacionPersonalizado)
                .GreaterThan(0).When(x => x.NumeroCotizacionPersonalizado.HasValue)
                .WithMessage("Si se especifica un número personalizado, debe ser mayor a cero.");
        }
    }
}
