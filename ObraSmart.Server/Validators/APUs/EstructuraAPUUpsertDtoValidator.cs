using FluentValidation;
using ObraSmart.Application.DTOs.APUs;

namespace ObraSmart.Server.Validators.APUs
{
    public class EstructuraAPUUpsertDtoValidator : AbstractValidator<EstructuraAPUUpsertDto>
    {
        public EstructuraAPUUpsertDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la estructura APU es obligatorio.");

            RuleFor(x => x.UnidadMedidaId)
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida válida.");

            RuleFor(x => x.Componentes)
                .NotEmpty().WithMessage("La receta del APU debe contener al menos un insumo.");

            // Validar cada elemento de la lista individualmente
            RuleForEach(x => x.Componentes).SetValidator(new ComponenteAPUInputDtoValidator());
        }
    }
}
