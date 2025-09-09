using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Internship
{
    public class InternshipDiaryRepository : Repository<InternshipDiary>, IInternshipDiaryRepository
    {
        private readonly ApplicationDbContext _context;

        public InternshipDiaryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<InternshipDiary>> GetDiariesByStudentIdAsync(int studentId)
        {
            return await _context.InternshipDiaries
                .Include(d => d.Student)
                .Include(d => d.Entries)
                .Where(d => d.StudentId == studentId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<InternshipDiary?> GetDiaryWithEntriesAsync(int diaryId)
        {
            return await _context.InternshipDiaries
                .Include(d => d.Student)
                .Include(d => d.Entries!.OrderBy(e => e.Date))
                .FirstOrDefaultAsync(d => d.Id == diaryId);
        }

        public async Task<List<InternshipDiary>> GetDiariesByUniversityAsync(int universityId)
        {
            return await _context.InternshipDiaries
                .Include(d => d.Student)
                .Include(d => d.Entries)
                .Where(d => d.Student!.UniversityId == universityId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateDiaryStatusAsync(int diaryId, bool isCompleted)
        {
            var diary = await _context.InternshipDiaries.FindAsync(diaryId);
            if (diary == null) return false;

            diary.IsCompleted = isCompleted;
            // PostgreSQL için UTC DateTime zorunlu
            diary.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}