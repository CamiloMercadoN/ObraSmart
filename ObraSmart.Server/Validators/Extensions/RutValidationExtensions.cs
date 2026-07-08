using FluentValidation;

namespace ObraSmart.Server.Validators.Extensions
{
    public static class RutValidationExtensions
    {
        // Extiende IRuleBuilder para que esté disponible en cualquier RuleFor que evalúe un string
        public static IRuleBuilderOptions<T, string> RutChilenoValido<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(ValidarRutChileno)
                              .WithMessage("El RUT ingresado no es válido.");
        }

        private static bool ValidarRutChileno(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut)) return false;

            var rutLimpio = new string(rut.Where(char.IsLetterOrDigit).ToArray()).ToUpper();
            if (rutLimpio.Length < 2) return false;

            var cuerpo = rutLimpio[..^1];
            var dv = rutLimpio.Substring(rutLimpio.Length - 1, 1);

            if (!int.TryParse(cuerpo, out _)) return false;

            int suma = 0;
            int multiplo = 2;

            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(cuerpo[i].ToString()) * multiplo;
                multiplo = multiplo < 7 ? multiplo + 1 : 2;
            }

            var dvEsperado = 11 - (suma % 11);
            var dvCalculado = dvEsperado == 11 ? "0" : dvEsperado == 10 ? "K" : dvEsperado.ToString();

            return dv == dvCalculado;
        }
    }
}
