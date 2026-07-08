using FluentValidation;
using ObraSmart.Application.DTOs;
using ObraSmart.Server.Validators.Extensions;

namespace ObraSmart.Server.Validators
{
    public class RegistroUsuarioValidator : AbstractValidator<RegistroUsuarioDto>
    {
        public RegistroUsuarioValidator()
        {
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .StrictEmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

            RuleFor(x => x.Rut)
                .NotEmpty().WithMessage("El RUT es obligatorio.")
                .RutChilenoValido();

            RuleFor(x => x.RazonSocial)
                .NotEmpty().WithMessage("La razón social es obligatoria.");
        }
    }
}
