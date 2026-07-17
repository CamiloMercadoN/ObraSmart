using FluentValidation;
using ObraSmart.Application.DTOs.Insumos;

namespace ObraSmart.Server.Validators.Insumos
{
    public class ActualizarPrecioDtoValidator : AbstractValidator<ActualizarPrecioDto>
    {
        public ActualizarPrecioDtoValidator()
        {
            RuleFor(x => x.NuevoPrecio)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El precio de referencia no puede ser negativo.");
        }
    }
}
