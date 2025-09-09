using Core.Entity;
using DataAccess.Context;
using DataAccess.Interfaces.Education;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repositories;

public class UniversityRepository : IUniversityRepository
{
    private readonly ApplicationDbContext _context;

    public UniversityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<University?> GetByIdAsync(int id)
    {
        return await _context.Universities.FindAsync(id);
    }

    public async Task<IEnumerable<University>> GetAllAsync()
    {
        return await _context.Universities.ToListAsync();
    }

    public async Task AddAsync(University entity)
    {
        await _context.Universities.AddAsync(entity);
    }

    public void Update(University entity)
    {
        _context.Universities.Update(entity);
    }

    public void Remove(University entity)
    {
        _context.Universities.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<University>> GetAllWithProvinceAsync()
    {
        return await _context.Universities
            .Include(u => u.Province)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<University>> GetByProvinceIdAsync(int provinceId)
    {
        return await _context.Universities
            .Where(u => u.ProvinceId == provinceId)
            .Include(u => u.Province)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<University?> GetByUserIdAsync(int userId)
    {
        // Kullanıcının UniversityId'sinden üniversiteyi getir
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.UniversityId == null) return null;

        return await _context.Universities
            .Include(u => u.Province)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.UniversityId.Value);
    }

    public async Task<University?> GetUniversityByNameAsync(string name)
    {
        return await _context.Universities
            .AsNoTracking()
            .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Name, name.Trim()));
    }


    public async Task<int> CountAsync(Expression<Func<University, bool>> predicate)
    {
        return await _context.Universities.CountAsync(predicate);
    }

    public async Task<List<University>> Where(Expression<Func<University, bool>> predicate)
    {
        return await _context.Universities.Where(predicate).ToListAsync();
    }
}