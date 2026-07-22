using FluentValidation;
using ObraSmart.Application.DTOs.Presupuestos;

namespace ObraSmart.Server.Validators.Presupuestos
{
    public class ItemPresupuestoUpsertDtoValidator : AbstractValidator<ItemPresupuestoUpsertDto>
    {
        public ItemPresupuestoUpsertDtoValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción del ítem es obligatoria.");

            RuleFor(x => x.CantidadItem)
                .GreaterThan(0).WithMessage("La cantidad del ítem debe ser mayor a cero.");

            RuleFor(x => x.UnidadMedidaId)
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida válida.");

            RuleForEach(x => x.Recursos).SetValidator(new RecursoItemPresupuestoUpsertDtoValidator());
        }
    }
}
