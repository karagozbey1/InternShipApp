using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Internship
{
    public class InternshipApplicationRepository : Repository<InternshipApplication>, IInternshipApplicationRepository
    {
        public InternshipApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Geçersiz öğrenci kimliği", nameof(studentId));

            // Include related Offer and Company so UI can display company name without additional queries
            // Fixed: Also include Student.University to show university information
            return await _context.InternshipApplications
                .Where(app => app.StudentId == studentId)
                .Include(a => a.InternshipOffer)
                    .ThenInclude(o => o.Company)
                .Include(a => a.Student)
                    .ThenInclude(s => s.University)
                .ToListAsync();
        }

        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            if (offerId <= 0)
                throw new ArgumentException("Geçersiz teklif kimliği", nameof(offerId));

            // Exclude rejected (withdrawn) applications so companies don't see withdrawn ones
            // Fixed: Include Student.University to show university information in company applications
            return await _context.InternshipApplications
                .Include(a => a.Student)
                    .ThenInclude(s => s.University)
                .Include(a => a.InternshipOffer).ThenInclude(o => o.Company)
                .Where(app => app.InternshipOfferId == offerId && app.Status != InternshipApplicationStatus.Rejected)
                .ToListAsync();
        }

        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            if (applicationId <= 0)
                throw new ArgumentException("Geçersiz başvuru kimliği", nameof(applicationId));

            return await _context.InternshipApplications
                .Include(a => a.Student)
                    .ThenInclude(s => s.University)
                .Include(a => a.InternshipOffer)
                    .ThenInclude(o => o.Company)
                .Include(a => a.Advisor)
                .Include(a => a.InternshipDocuments)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Geçersiz durum", nameof(status));

            if (!Enum.TryParse<InternshipApplicationStatus>(status, true, out var parsedStatus))
                throw new ArgumentException("Geçersiz başvuru durumu", nameof(status));

            var statusValue = (int)parsedStatus;

            return await _context.InternshipApplications
                .Where(app => (int)app.Status == statusValue)
                .ToListAsync();
        }

        public async Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId)
        {
            if (advisorId <= 0)
                throw new ArgumentException("Geçersiz danışman kimliği", nameof(advisorId));

            return await _context.InternshipApplications
                .Where(a => a.AdvisorId == advisorId)
                .ToListAsync();
        }
        public async Task<List<InternshipApplication>> GetApprovedByStudentIdAsync(int studentId)
        {
            return await _context.InternshipApplications
                .Include(x => x.InternshipOffer).ThenInclude(o => o.Company)
                .Where(x => x.StudentId == studentId && x.Status == InternshipApplicationStatus.Approved)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

    }
}
