
namespace ObraSmart.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task<string> GuardarImagenBase64Async(string base64String, string carpetaDestino, string prefijoArchivo);
        void EliminarArchivo(string rutaRelativa);
    }
}
