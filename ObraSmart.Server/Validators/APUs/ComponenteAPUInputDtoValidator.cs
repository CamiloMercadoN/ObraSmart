using FluentValidation;
using ObraSmart.Application.DTOs.APUs;

namespace ObraSmart.Server.Validators.APUs
{
    public class ComponenteAPUInputDtoValidator : AbstractValidator<ComponenteAPUInputDto>
    {
        public ComponenteAPUInputDtoValidator()
        {
            RuleFor(x => x.InsumoId)
                .NotEmpty().WithMessage("El ID del insumo es obligatorio.");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad o rendimiento del insumo debe ser mayor a cero.");
        }
    }
}
