using Core.Entity;
using DataAccess.Context;
using DataAccess.Interfaces.Education;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repositories;

public class ProvinceRepository : IProvinceRepository
{
    private readonly ApplicationDbContext _context;

    public ProvinceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Province?> GetByIdAsync(int id)
    {
        return await _context.Provinces.FindAsync(id);
    }

    public async Task<IEnumerable<Province>> GetAllAsync()
    {
        return await _context.Provinces.ToListAsync();
    }

    public async Task AddAsync(Province entity)
    {
        await _context.Provinces.AddAsync(entity);
    }

    public void Update(Province entity)
    {
        _context.Provinces.Update(entity);
    }

    public void Remove(Province entity)
    {
        _context.Provinces.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Province>> GetByStartingLetterAsync(char letter)
    {
        var pattern = letter.ToString().ToUpper() + "%";
        return await _context.Provinces
            .Where(p => EF.Functions.ILike(p.Name, pattern))
            .AsNoTracking()
            .ToListAsync();
    }

    
    public async Task<int> CountAsync(Expression<Func<Province, bool>> predicate)
    {
        return await _context.Provinces.CountAsync(predicate);
    }

 
    public async Task<List<Province>> Where(Expression<Func<Province, bool>> predicate)
    {
        return await _context.Provinces.Where(predicate).ToListAsync();
    }
}