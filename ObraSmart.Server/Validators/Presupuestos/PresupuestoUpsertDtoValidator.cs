using FluentValidation;
using ObraSmart.Application.DTOs.Presupuestos;

namespace ObraSmart.Server.Validators.Presupuestos
{
    public class PresupuestoUpsertDtoValidator : AbstractValidator<PresupuestoUpsertDto>
    {
        public PresupuestoUpsertDtoValidator()
        {
            RuleFor(x => x.NombreProyecto)
                .NotEmpty().WithMessage("El nombre del proyecto es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre del proyecto no puede superar los 150 caracteres.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("El presupuesto debe contener al menos un ítem.")
                .Must(items => items != null && items.Count > 0).WithMessage("Debe agregar al menos una estructura al presupuesto.");

            RuleForEach(x => x.Items).SetValidator(new ItemPresupuestoUpsertDtoValidator());
        }
    }
}
