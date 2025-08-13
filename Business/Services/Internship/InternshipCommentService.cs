using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    public class InternshipCommentService : IInternshipCommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InternshipCommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InternshipComment?> GetCommentByIdAsync(int commentId)
        {
            return await _unitOfWork.InternshipComments.GetByIdAsync(commentId);
        }

        public async Task<List<InternshipComment>> GetCommentsByApplicationIdAsync(int applicationId)
        {
            return await _unitOfWork.InternshipComments.GetCommentsByApplicationIdAsync(applicationId);
        }

        public async Task<InternshipComment> AddCommentAsync(InternshipComment comment)
        {
            comment.SentAt = DateTime.UtcNow;
            await _unitOfWork.InternshipComments.AddAsync(comment);
            await _unitOfWork.SaveAsync();
            return comment;
        }

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