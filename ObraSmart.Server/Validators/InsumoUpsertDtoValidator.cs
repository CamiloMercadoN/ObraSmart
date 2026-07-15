using FluentValidation;
using ObraSmart.Application.DTOs.Insumos;

namespace ObraSmart.Server.Validators
{
    public class InsumoUpsertDtoValidator : AbstractValidator<InsumoUpsertDto>
    {
        public InsumoUpsertDtoValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción del insumo es obligatoria.")
                .MaximumLength(200).WithMessage("La descripción no puede exceder los 200 caracteres.");

            RuleFor(x => x.TipoInsumo)
                .NotEmpty().WithMessage("El tipo de insumo es obligatorio.")
                // Validar contra catálogo de tipos (ej. Material, Mano de Obra, Equipo)
                .Must(tipo => new[] { "Material", "Mano de Obra", "Equipo" }.Contains(tipo))
                .WithMessage("El tipo de insumo no es válido.");

            RuleFor(x => x.PrecioReferencia)
                .GreaterThanOrEqualTo(0).WithMessage("El precio de referencia no puede ser negativo.");

            RuleFor(x => x.UnidadMedidaId)
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida válida.");

            RuleFor(x => x.EtiquetasIds)
                .NotNull().WithMessage("La lista de etiquetas no puede ser nula.");
        }
    }
}
