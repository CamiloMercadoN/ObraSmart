

namespace ObraSmart.Domain.Interfaces
{
    public interface IUserOwnedEntity
    {
        Guid UsuarioId { get; set; }
        bool EsPlantilla { get; set; }
    }
}
