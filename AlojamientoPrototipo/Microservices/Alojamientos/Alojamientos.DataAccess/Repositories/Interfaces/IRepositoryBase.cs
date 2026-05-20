using Alojamientos.DataAccess.Common;
using Alojamientos.DataAccess.Entities;

namespace Alojamientos.DataAccess.Repositories.Interfaces;

public interface IRepositoryBase<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync();
    Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
}
