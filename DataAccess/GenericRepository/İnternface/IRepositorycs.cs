using System.Linq.Expressions;

namespace DataAccess.GenericRepository.İnternface;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> Where(Expression<Func<T, bool>> predicate);
    Task SaveChangesAsync();
}
