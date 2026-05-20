using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Reservas.DataAccess.Common;

/// <summary>
/// Repositorio base genérico con operaciones CRUD estándar.
/// </summary>
public abstract class RepositoryBase<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected RepositoryBase(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<int> CountAsync()
        => await _dbSet.CountAsync();

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.CountAsync(predicate);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
