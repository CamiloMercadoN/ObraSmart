
namespace ObraSmart.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerarToken(Guid usuarioId, string correo, string razonSocial);
    }
}
