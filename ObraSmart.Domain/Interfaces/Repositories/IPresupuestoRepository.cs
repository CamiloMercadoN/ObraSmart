using ObraSmart.Domain.Entities;

namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IPresupuestoRepository : IRepository<Presupuesto, Guid>
    {
        Task<IReadOnlyList<Presupuesto>> GetAllWithDependenciesAsync();
        Task<Presupuesto?> GetByIdWithDependenciesAsync(Guid id);
        Task UpdateGrafoAsync(Presupuesto presupuesto, IEnumerable<ItemPresupuesto> itemsAEliminar, IEnumerable<RecursoItemPresupuesto> recursosAEliminar);
        Task DeleteGrafoAsync(Presupuesto presupuesto);
    }
}
