using FluentValidation;
using ObraSmart.Application.DTOs.Presupuestos;

namespace ObraSmart.Server.Validators.Presupuestos
{
    public class RecursoItemPresupuestoUpsertDtoValidator : AbstractValidator<RecursoItemPresupuestoUpsertDto>
    {
        public RecursoItemPresupuestoUpsertDtoValidator()
        {
            RuleFor(x => x.TipoInsumo)
                .NotEmpty().WithMessage("El tipo de insumo es obligatorio.");

            RuleFor(x => x.DescripcionCongelada)
                .NotEmpty().WithMessage("La descripción del recurso es obligatoria.");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad del recurso debe ser mayor a cero.");

            RuleFor(x => x.PrecioUnitarioCongelado)
                .GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");
        }
    }
}
