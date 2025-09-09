using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Internship
{
    public class InternshipPeriodRepository : Repository<InternshipPeriod>, IInternshipPeriodRepository
    {
        public InternshipPeriodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<InternshipPeriod>> GetActivePeriodsAsync()
        {
            // Show current and upcoming periods (end date in future)
            return await _context.InternshipPeriods
                                 .Where(p => p.EndDate >= DateTime.UtcNow)
                                 .ToListAsync();
        }

        public async Task<InternshipPeriod?> GetPeriodByNameAsync(string periodName)
        {
            return await _context.InternshipPeriods
                                 .FirstOrDefaultAsync(p => p.PeriodName == periodName);
        }

        public async Task<InternshipPeriod?> GetActivePeriodByStudentAsync(int studentId)
        {
            // Find a period where the student is assigned and period is active
            return await _context.InternshipPeriods
                .Where(p => p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .Where(p => p.PeriodStudents.Any(ps => ps.StudentId == studentId))
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCompletedDaysCountAsync(int periodId)
        {
            var period = await _context.InternshipPeriods.FindAsync(periodId);
            if (period == null) return 0;

            
            var completedDays = (int)(DateTime.UtcNow - period.StartDate).TotalDays;

            var totalDays = (int)(period.EndDate - period.StartDate).TotalDays;

            
            return Math.Max(0, Math.Min(completedDays, totalDays));
        }

        public async Task<List<InternshipPeriod>> GetPeriodsByStudentIdAsync(int studentId)
        {
            return await _context.InternshipPeriods
                .Where(p => p.PeriodStudents.Any(ps => ps.StudentId == studentId))
                .ToListAsync();
        }

        public async Task<bool> IsStudentInPeriodAsync(int periodId, int studentId)
        {
            return await _context.InternshipPeriodStudents
                .AnyAsync(ps => ps.InternshipPeriodId == periodId && ps.StudentId == studentId);
        }

        public async Task AddStudentToPeriodAsync(int periodId, int studentId)
        {
            var exists = await IsStudentInPeriodAsync(periodId, studentId);
            if (exists) return;

            var entry = new InternshipPeriodStudent
            {
                InternshipPeriodId = periodId,
                StudentId = studentId
            };

            await _context.InternshipPeriodStudents.AddAsync(entry);
            // Note: do not Save here; caller should Save via UnitOfWork
        }
    }
}