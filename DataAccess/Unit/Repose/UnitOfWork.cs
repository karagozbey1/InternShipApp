using DataAccess.Context;
using DataAccess.Interfaces;
using DataAccess.Interfaces.Chat;
using DataAccess.Interfaces.Com;
using DataAccess.Interfaces.Education;
using DataAccess.Interfaces.Internship;
using DataAccess.Interfaces.Order;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Repositories;
using DataAccess.Repositories.Chat;
using DataAccess.Repositories.Com;
using DataAccess.Repositories.Internship;
using DataAccess.Repositories.Order;
using DataAccess.Unit.Interface;

namespace DataAccess.Unit.Repose
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            InternshipPeriods = new InternshipPeriodRepository(_context);
            InternshipDocuments = new InternshipDocumentRepository(_context);
            InternshipDiaries = new InternshipDiaryRepository(_context);
            Companies = new CompanyRepository(_context);
            InternshipOffers = new InternshipOfferRepository(_context);
            InternshipApplications = new InternshipApplicationRepository(_context);
            InternshipApprovals = new InternshipApprovalRepository(_context);
            News = new NewsRepository(_context);
            Notifications = new NotificationRepository(_context);
            Users = new UserRepository(_context);
            ChatRooms = new ChatRoomRepository(_context);
            ChatMessages = new ChatMessageRepository(_context);
            CompanyUsers = new CompanyUserRepository(_context);
            Sliders = new SliderRepository(_context);
            SliderItems = new SliderItemRepository(_context);
            InternshipComments = new InternshipCommentRepository(_context);
            Provinces = new ProvinceRepository(_context);
            Universities = new UniversityRepository(_context);
            DiaryRepository = new DiaryRepository(_context);
            DiaryEntries = new DiaryRepository(_context);
        }

        public IDiaryRepository DiaryRepository { get; }
        public IDiaryRepository DiaryEntries { get; }
        public IInternshipDiaryRepository InternshipDiaries { get; }
        public ICompanyRepository Companies { get; }
        public IInternshipOfferRepository InternshipOffers { get; }
        public IInternshipApplicationRepository InternshipApplications { get; }
        public IInternshipApprovalRepository InternshipApprovals { get; }
        public IInternshipCommentRepository InternshipComments { get; }
        public IInternshipDocumentRepository InternshipDocuments { get; }
        public IInternshipPeriodRepository InternshipPeriods { get; }
        public INewsRepository News { get; }
        public INotificationRepository Notifications { get; }
        public IUserRepository Users { get; }
        public IChatRoomRepository ChatRooms { get; }
        public IChatMessageRepository ChatMessages { get; }
        public ICompanyUserRepository CompanyUsers { get; }
        public ISliderRepository Sliders { get; }
        public ISliderItemRepository SliderItems { get; }
        public IProvinceRepository Provinces { get; private set; }
        public IUniversityRepository Universities { get; private set; }

        public async Task<int> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Log detailed DB error server-side
                Console.WriteLine($"DbUpdateException in UnitOfWork.SaveAsync: {dbEx}");
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                throw new InvalidOperationException($"Database save failed: {inner}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UnitOfWork.SaveAsync: {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
