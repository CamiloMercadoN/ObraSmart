using FluentValidation;
using ObraSmart.Application.DTOs.Clientes;
using ObraSmart.Server.Validators.Extensions;

namespace ObraSmart.Server.Validators.Clientes
{
    public class ClienteValidator : AbstractValidator<ClienteRequestDto>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede exceder los 150 caracteres.");

            RuleFor(x => x.Rut)
                .RutChilenoValido()
                .When(x => !string.IsNullOrEmpty(x.Rut));

            RuleFor(x => x.Correo)
                .StrictEmailAddress()
                .When(x => !string.IsNullOrEmpty(x.Correo));
        }
    }
}
