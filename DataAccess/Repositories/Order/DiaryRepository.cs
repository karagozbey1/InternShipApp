using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.İnternface;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class DiaryRepository : IRepository<DiaryEntry>, IDiaryRepository
    {
        private readonly ApplicationDbContext _context;

        public DiaryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DiaryEntry entity)
        {
            await _context.Set<DiaryEntry>().AddAsync(entity);
        }

        public async Task<int> CountAsync(Expression<Func<DiaryEntry, bool>> predicate)
        {
            return await _context.Set<DiaryEntry>().CountAsync(predicate);
        }

        public async Task<IEnumerable<DiaryEntry>> GetAllAsync()
        {
            return await _context.Set<DiaryEntry>().ToListAsync();
        }

        public async Task<DiaryEntry?> GetByIdAsync(int id)
        {
            return await _context.Set<DiaryEntry>().FindAsync(id);
        }

        public void Remove(DiaryEntry entity)
        {
            _context.Set<DiaryEntry>().Remove(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(DiaryEntry entity)
        {
            _context.Set<DiaryEntry>().Update(entity);
        }

        public async Task<List<DiaryEntry>> Where(Expression<Func<DiaryEntry, bool>> predicate)
        {
            return await _context.Set<DiaryEntry>().Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<DiaryEntry>> GetDiaryEntriesByStudentIdAsync(int studentId, int? take = null)
        {
            var query = _context.Set<DiaryEntry>()
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.Id);

            if (take.HasValue)
            {
                query = (IOrderedQueryable<DiaryEntry>)query.Take(take.Value);
            }

            return await query.ToListAsync();
        }
    }
}
