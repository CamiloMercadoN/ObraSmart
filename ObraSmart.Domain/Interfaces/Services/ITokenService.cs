using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerarToken(Guid usuarioId, string correo);
    }
}
