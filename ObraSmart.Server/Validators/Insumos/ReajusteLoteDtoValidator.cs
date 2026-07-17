using FluentValidation;
using ObraSmart.Application.DTOs.Insumos;

namespace ObraSmart.Server.Validators.Insumos
{
    public class ReajusteLoteDtoValidator : AbstractValidator<ReajusteLoteDto>
    {
        public ReajusteLoteDtoValidator()
        {
            RuleFor(x => x.Valor)
                .NotEmpty().WithMessage("El valor de reajuste es obligatorio.")
                .NotEqual(0).WithMessage("El valor de reajuste no puede ser cero.");

            RuleFor(x => x.Valor)
                .GreaterThanOrEqualTo(-100)
                .When(x => x.EsPorcentaje)
                .WithMessage("El porcentaje de descuento no puede superar el -100%.");
        }
    }
}
