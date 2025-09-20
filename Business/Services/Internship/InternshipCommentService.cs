using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    // Yorum servisi
    public class InternshipCommentService : IInternshipCommentService
    {
        // Veritabanı işlemleri
        private readonly IUnitOfWork _unitOfWork;

        // Kurucu metod
        public InternshipCommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Kimlik ile yorum getirir
        public async Task<InternshipComment?> GetCommentByIdAsync(int commentId)
        {
            return await _unitOfWork.InternshipComments.GetByIdAsync(commentId);
        }

        // Başvuru yorumlarını getirir
        public async Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId)
        {
            return await _unitOfWork.InternshipComments.GetCommentsByApplicationIdAsync(applicationId);
        }

        // Yorum ekler
        public async Task<InternshipComment> AddCommentAsync(InternshipComment comment)
        {
            comment.SentAt = DateTime.UtcNow;
            await _unitOfWork.InternshipComments.AddAsync(comment);
            await _unitOfWork.SaveAsync();
            return comment;
        }

        // Yorum siler
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _unitOfWork.InternshipComments.GetByIdAsync(commentId);
            if (comment != null)
            {
                _unitOfWork.InternshipComments.Remove(comment);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}