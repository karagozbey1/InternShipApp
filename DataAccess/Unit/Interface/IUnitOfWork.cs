using DataAccess.Interfaces.Chat;
using DataAccess.Interfaces.Com;
using DataAccess.Interfaces.Internship;
using DataAccess.Interfaces.Order;
using DataAccess.Interfaces.UserRepose;
using System;
using System.Threading.Tasks;

namespace DataAccess.Unit.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        IInternshipOfferRepository InternshipOffers { get; }
        IInternshipCommentRepository InternshipComments { get; }
        IInternshipDocumentRepository InternshipDocuments { get; }
        IInternshipPeriodRepository InternshipPeriods { get; }
        IInternshipApplicationRepository InternshipApplications { get; }
        INewsRepository News { get; }
        INotificationRepository Notifications { get; }
        IUserRepository Users { get; }
        IChatRoomRepository ChatRooms { get; }
        IChatMessageRepository ChatMessages { get; }
        ICompanyUserRepository CompanyUsers { get; }
        ISliderRepository Sliders { get; }
        ISliderItemRepository SliderItems { get; }
        
        Task<int> SaveAsync();
    }
}