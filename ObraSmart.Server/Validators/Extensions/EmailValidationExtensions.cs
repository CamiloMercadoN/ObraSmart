using FluentValidation;

namespace ObraSmart.Server.Validators.Extensions
{
    public static class EmailValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> StrictEmailAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                .WithMessage("El formato del correo es inválido (ejemplo@dominio.com).");
        }
    }
}
