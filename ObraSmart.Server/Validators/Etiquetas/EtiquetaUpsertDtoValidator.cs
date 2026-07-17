using FluentValidation;
using ObraSmart.Application.DTOs.Etiquetas;

namespace ObraSmart.Server.Validators.Etiquetas
{
    public class EtiquetaUpsertDtoValidator : AbstractValidator<EtiquetaUpsertDto>
    {
        public EtiquetaUpsertDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la etiqueta es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no puede superar los 50 caracteres.");

            RuleFor(x => x.ColorHex)
                .NotEmpty().WithMessage("El color hexadecimal es obligatorio.")
                .Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")
                .WithMessage("El color debe tener un formato hexadecimal válido (ej. #FF5733).");
        }
    }
}
