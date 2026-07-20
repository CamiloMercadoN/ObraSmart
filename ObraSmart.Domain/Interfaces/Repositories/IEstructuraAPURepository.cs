using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IEstructuraAPURepository : IRepository<EstructuraAPU, Guid>
    {
        Task<IReadOnlyList<EstructuraAPU>> GetAllWithDependenciesAsync();
        Task<EstructuraAPU?> GetByIdWithDependenciesAsync(Guid id);
        Task UpdateGrafoAsync(EstructuraAPU apu, IEnumerable<ComponenteAPU> componentesAEliminar);
        Task DeleteGrafoAsync(EstructuraAPU apu);
    }
}
