using DataAccess.Context;
using DataAccess.GenericRepository.İnternface;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.GenericRepository.Repository;

public class Repository<T> : IRepository<T> where T : class
{
    //diğer Sınıflar Repositoryden Miras Aldıgı için   _context diğer sınıflarda tekrar tanımlanmadan kullanılır ;
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
