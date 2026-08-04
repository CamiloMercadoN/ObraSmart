using FluentValidation;
using ObraSmart.Application.DTOs.ConfiguracionComercial;

namespace ObraSmart.Server.Validators.ConfiguracionComercial
{
    public class ConfiguracionComercialDtoValidator : AbstractValidator<ConfiguracionComercialDto>
    {
        public ConfiguracionComercialDtoValidator()
        {
            RuleFor(x => x.RazonSocial)
                .NotEmpty().WithMessage("La razón social es obligatoria.")
                .MaximumLength(150).WithMessage("La razón social no puede superar los 150 caracteres.");

            RuleFor(x => x.PorcentajeIva)
                .InclusiveBetween(0, 100).WithMessage("El IVA debe estar entre 0 y 100.");

            RuleFor(x => x.DiasValidez)
                .InclusiveBetween(1, 365).WithMessage("Los días de validez deben estar entre 1 y 365.");
        }
    }
}
