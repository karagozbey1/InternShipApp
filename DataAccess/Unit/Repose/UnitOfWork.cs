using DataAccess.Context;
using DataAccess.Interfaces.Chat;
using DataAccess.Interfaces.Com;
using DataAccess.Interfaces.Internship;
using DataAccess.Interfaces.Order;
using DataAccess.Interfaces.UserRepose;
using DataAccess.Repositories.Chat;
using DataAccess.Repositories.Com;
using DataAccess.Repositories.Internship;
using DataAccess.Repositories.Order;
using DataAccess.Unit.Interface;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        InternshipPeriods = new InternshipPeriodRepository(_context);
        InternshipDocuments = new InternshipDocumentRepository(_context);
        Companies = new CompanyRepository(_context);
        InternshipOffers = new InternshipOfferRepository(_context);
        InternshipApplications = new InternshipApplicationRepository(_context);
        News = new NewsRepository(_context);
        Notifications = new NotificationRepository(_context);
        Users = new UserRepository(_context);
        ChatRooms = new ChatRoomRepository(_context);
        ChatMessages = new ChatMessageRepository(_context);
        CompanyUsers = new CompanyUserRepository(_context);
        Sliders = new SliderRepository(_context);
        SliderItems = new SliderItemRepository(_context);
        InternshipComments = new InternshipCommentRepository(_context);
    }

    // ActivationEmails kaldırıldı!

    public ICompanyRepository Companies { get; }
    public IInternshipOfferRepository InternshipOffers { get; }
    public IInternshipApplicationRepository InternshipApplications { get; }
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

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
