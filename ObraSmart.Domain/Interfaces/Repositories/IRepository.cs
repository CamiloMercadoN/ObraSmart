
namespace ObraSmart.Domain.Interfaces.Repositories
{
    public interface IRepository<TEntity, TId> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TId id);
        Task<IReadOnlyList<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}
