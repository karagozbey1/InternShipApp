using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Internship
{
    public class InternshipApplicationRepository : Repository<InternshipApplication>, IInternshipApplicationRepository
    {
        

        public InternshipApplicationRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            return await _context.InternshipApplications
                .Where(app => app.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            return await _context.InternshipApplications
                .Where(app => app.InternshipOfferId == offerId)
                .ToListAsync();
        }

        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
            return await _context.InternshipApplications
                .Include(a => a.Student)
                .Include(a => a.InternshipOffer)
                .Include(a => a.Advisor)
                .FirstOrDefaultAsync(a => a.Id == applicationId);
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            return await _context.InternshipApplications
                .Where(app => app.Status == status)
                .ToListAsync();
        }


        public async Task<List<InternshipApplication>> GetApplicationsByAdvisorIdAsync(int advisorId)
        {
            return await _context.InternshipApplications
                                 .Where(a => a.AdvisorId == advisorId)
                                 .ToListAsync();
        }
    }
}