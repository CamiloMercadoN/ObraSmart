using Microsoft.EntityFrameworkCore;
using ObraSmart.Domain.Interfaces.Repositories;
using ObraSmart.Infrastructure.Data;

namespace ObraSmart.Infrastructure.Repositories
{
    public class RepositoryBase<TEntity, TId>(ObraSmartDbContext context) : IRepository<TEntity, TId> where TEntity : class
    {
        protected readonly ObraSmartDbContext _context = context;

        public virtual async Task<TEntity?> GetByIdAsync(TId id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().AsNoTracking().ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
