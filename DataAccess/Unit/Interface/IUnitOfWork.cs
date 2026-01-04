using DataAccess.Interfaces;
using DataAccess.Interfaces.Chat;
using DataAccess.Interfaces.Com;

using DataAccess.Interfaces.Education;
using DataAccess.Interfaces.Internship;
using DataAccess.Interfaces.Order;
using DataAccess.Interfaces.UserRepose;
using System;
using System.Threading.Tasks;

namespace DataAccess.Unit.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IDiaryRepository DiaryRepository { get; }
        IDiaryRepository DiaryEntries { get; }
        IInternshipDiaryRepository InternshipDiaries { get; }
        ICompanyRepository Companies { get; }
        IInternshipOfferRepository InternshipOffers { get; }
        IInternshipCommentRepository InternshipComments { get; }
        IInternshipDocumentRepository InternshipDocuments { get; }
        IInternshipPeriodRepository InternshipPeriods { get; }
        IInternshipApplicationRepository InternshipApplications { get; }
        IInternshipApprovalRepository InternshipApprovals { get; }
        IInternEvaluationRepository InternEvaluations { get; }
        INotificationRepository Notifications { get; }
        IUserRepository Users { get; }
        IChatRoomRepository ChatRooms { get; }
        IChatMessageRepository ChatMessages { get; }
        ICompanyUserRepository CompanyUsers { get; }
        IProvinceRepository Provinces { get; }
        IUniversityRepository Universities { get; }
        IOfferFavoriteRepository OfferFavorites { get; }
        Task<int> SaveAsync();
    }
}