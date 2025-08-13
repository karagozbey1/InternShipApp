using Business.Interfaces.Internship;
using Core.Entity;
using DataAccess.Unit.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.Services.Internship
{
    public class InternshipApplicationService : IInternshipApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InternshipApplicationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStudentIdAsync(int studentId)
        {
            return await _unitOfWork.InternshipApplications.GetApplicationsByStudentIdAsync(studentId);
        }

        public async Task<List<InternshipApplication>> GetApplicationsByOfferIdAsync(int offerId)
        {
            return await _unitOfWork.InternshipApplications.GetApplicationsByOfferIdAsync(offerId);
        }

        public async Task<InternshipApplication?> GetApplicationWithDetailsAsync(int applicationId)
        {
           
            return await _unitOfWork.InternshipApplications.GetApplicationWithDetailsAsync(applicationId);
        }

        public async Task<List<InternshipApplication>> GetApplicationsByStatusAsync(string status)
        {
            return await _unitOfWork.InternshipApplications.GetApplicationsByStatusAsync(status);
        }

        public async Task<InternshipApplication> AddApplicationAsync(InternshipApplication application)
        {
            // İş mantığı: Başvuru oluşturulmadan önce öğrencinin durumu veya staj teklifinin geçerliliği kontrol edilebilir.
            application.Status = "Pending";
            await _unitOfWork.InternshipApplications.AddAsync(application);
            await _unitOfWork.SaveAsync();
            return application;
        }

        public async Task UpdateApplicationStatusAsync(int applicationId, string newStatus)
        {
            // İş mantığı: Durum değişikliği için geçerli durumlar listesi kontrol edilebilir.
            var application = await _unitOfWork.InternshipApplications.GetByIdAsync(applicationId);
            if (application != null)
            {
                application.Status = newStatus;
                _unitOfWork.InternshipApplications.Update(application);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}