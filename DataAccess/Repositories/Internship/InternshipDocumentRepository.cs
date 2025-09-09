using Core.Entity;
using DataAccess.Context;
using DataAccess.GenericRepository.Repository;
using DataAccess.Interfaces.Internship;
using DataAccess.Unit.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Internship;

public class InternshipDocumentRepository : Repository<InternshipDocument>, IInternshipDocumentRepository
{
    public InternshipDocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
    {
        return await _context.InternshipDocuments
                             .Where(d => d.ApplicationId == applicationId)
                             .OrderByDescending(d => d.UploadedAt)
                             .ToListAsync();
    }

    public async Task<List<InternshipDocument>> GetCompanyDocumentsAsync(int companyId)
    {
        return await _context.InternshipDocuments
            .Where(d => d.CompanyId == companyId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<InternshipDocument>> GetDiaryEntriesByStudentIdAsync(int studentId)
    {
        return await _context.InternshipDocuments
            .Where(d => d.StudentId == studentId && d.DocumentType == "Diary")
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    // Öğrencinin tüm belgelerini getir (başvuru bağımsız dahil)
    public async Task<List<InternshipDocument>> GetDocumentsByStudentIdAsync(int studentId)
    {
        return await _context.InternshipDocuments
            .Where(d => d.StudentId == studentId || 
                       (d.ApplicationId.HasValue && 
                        _context.InternshipApplications.Any(app => app.Id == d.ApplicationId && app.StudentId == studentId)))
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    // Başvuru bağımsız belgeleri getir (sadece StudentId ile ilişkili, ApplicationId null olanlar)
    public async Task<List<InternshipDocument>> GetIndependentDocumentsByStudentIdAsync(int studentId)
    {
        return await _context.InternshipDocuments
            .Where(d => d.StudentId == studentId && d.ApplicationId == null)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task LinkDocumentToApprovalAsync(int documentId, int approvalFormId)
    {
        var document = await _context.InternshipDocuments.FindAsync(documentId);
        if (document != null)
        {
            // Set the ApprovalFormId to create a real relation
            document.ApprovalFormId = approvalFormId;
            document.Description = (document.Description ?? "") + $" [Onay Formu: {approvalFormId}]";
            _context.InternshipDocuments.Update(document);
        }
    }

    // Yeni: Onay formuna bağlı belgeleri getir
    public async Task<List<InternshipDocument>> GetDocumentsByApprovalFormIdAsync(int approvalFormId)
    {
        return await _context.InternshipDocuments
            .Where(d => d.ApprovalFormId == approvalFormId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }
}