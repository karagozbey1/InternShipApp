using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    public class InternshipDocumentService : IInternshipDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InternshipDocumentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InternshipDocument?> GetDocumentByIdAsync(int documentId)
        {
            return await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
        }

        public async Task<List<InternshipDocument>> GetDocumentsByApplicationIdAsync(int applicationId)
        {
            return await _unitOfWork.InternshipDocuments.GetDocumentsByApplicationIdAsync(applicationId);
        }

        public async Task<InternshipDocument> AddDocumentAsync(InternshipDocument document)
        {
            document.UploadedAt = DateTime.UtcNow;
            await _unitOfWork.InternshipDocuments.AddAsync(document);
            await _unitOfWork.SaveAsync();
            return document;
        }

        public async Task DeleteDocumentAsync(int documentId)
        {
            var document = await _unitOfWork.InternshipDocuments.GetByIdAsync(documentId);
            if (document != null)
            {
                _unitOfWork.InternshipDocuments.Remove(document);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}