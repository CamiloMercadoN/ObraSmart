using Microsoft.Extensions.Options;
using ObraSmart.Application.Interfaces.Services;
using ObraSmart.Infrastructure.Options;
using System.Text.RegularExpressions;

namespace ObraSmart.Infrastructure.Services
{
    public class LocalFileService(IOptions<StorageOptions> options) : IFileService
    {
        private readonly string _basePath = options.Value.BasePath;

        public async Task<string> GuardarImagenBase64Async(string base64String, string carpetaDestino, string prefijoArchivo)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return string.Empty;

            var match = Regex.Match(base64String, @"^data:image/(?<type>.+?);base64,(?<data>.+)$");

            if (!match.Success)
                return string.Empty;

            string extension = match.Groups["type"].Value;
            string datosBase64 = match.Groups["data"].Value;

            if (extension == "jpeg") extension = "jpg";

            string nombreArchivo = $"{prefijoArchivo}-{Guid.NewGuid().ToString().Substring(0, 8)}.{extension}";

            string rutaFisicaDirectorio = Path.Combine(_basePath, carpetaDestino);

            if (!Directory.Exists(rutaFisicaDirectorio))
            {
                Directory.CreateDirectory(rutaFisicaDirectorio);
            }

            string rutaFisicaArchivo = Path.Combine(rutaFisicaDirectorio, nombreArchivo);

            byte[] imageBytes = Convert.FromBase64String(datosBase64);
            await File.WriteAllBytesAsync(rutaFisicaArchivo, imageBytes);

            return $"/{carpetaDestino}/{nombreArchivo}".Replace("\\", "/");
        }

        public void EliminarArchivo(string rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return;

            string rutaSinSlash = rutaRelativa.TrimStart('/');
            string rutaFisica = Path.Combine(_basePath, rutaSinSlash);

            if (File.Exists(rutaFisica))
            {
                File.Delete(rutaFisica);
            }
        }
    }
}
