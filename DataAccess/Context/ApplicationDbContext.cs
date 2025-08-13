
using Core.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<CompanyUser> CompanyUsers { get; set; } = null!;
    public DbSet<InternshipPeriod> InternshipPeriods { get; set; } = null!;
    public DbSet<InternshipOffer> InternshipOffers { get; set; } = null!;
    public DbSet<InternshipApplication> InternshipApplications { get; set; } = null!;
    public DbSet<InternshipDocument> InternshipDocuments { get; set; } = null!;
    public DbSet<InternshipComment> InternshipComments { get; set; } = null!;
    public DbSet<News> News { get; set; } = null!;
    public DbSet<ChatRoom> ChatRooms { get; set; } = null!;
    public DbSet<UserChatRoom> UserChatRooms { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<SliderItem> SliderItems => Set<SliderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
          
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email)
                  .HasMaxLength(256)
                  .IsRequired();

        
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(50)
                  .IsRequired();
        });

        modelBuilder.Entity<CompanyUser>()
            .HasIndex(cu => new { cu.CompanyId, cu.UserId })
            .IsUnique();

        modelBuilder.Entity<CompanyUser>()
            .HasOne(cu => cu.Company)
            .WithMany(c => c.CompanyUsers)
            .HasForeignKey(cu => cu.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CompanyUser>()
            .HasOne(cu => cu.User)
            .WithMany(u => u.CompanyUsers)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternshipOffer>()
            .HasOne(o => o.Company)
            .WithMany(c => c.InternshipOffers)
            .HasForeignKey(o => o.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternshipOffer>()
            .HasOne(o => o.Period)
            .WithMany(p => p.InternshipOffers)
            .HasForeignKey(o => o.PeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternshipApplication>()
            .HasOne(a => a.Student)
            .WithMany(u => u.StudentApplications)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternshipApplication>()
            .HasOne(a => a.Advisor)
            .WithMany(u => u.AdvisorApplications)
            .HasForeignKey(a => a.AdvisorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternshipApplication>()
            .HasOne(a => a.InternshipOffer)
            .WithMany(o => o.InternshipApplications)
            .HasForeignKey(a => a.InternshipOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternshipComment>()
            .HasOne(c => c.InternshipApplication)
            .WithMany(a => a.InternshipComments)
            .HasForeignKey(c => c.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InternshipComment>()
            .HasOne(c => c.Sender)
            .WithMany(u => u.SentComments)
            .HasForeignKey(c => c.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InternshipDocument>()
            .HasOne(d => d.InternshipApplication)
            .WithMany(a => a.InternshipDocuments)
            .HasForeignKey(d => d.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
       
        modelBuilder.Entity<InternshipPeriod>()
           .HasKey(p => p.PeriodId);

        modelBuilder.Entity<News>()
            .HasIndex(n => new { n.Category, n.PublishDate });

        modelBuilder.Entity<ChatRoom>()
            .HasIndex(cr => cr.RoomIdentifier)
            .IsUnique();

        modelBuilder.Entity<UserChatRoom>()
            .HasKey(uc => new { uc.UserId, uc.ChatRoomId });

        modelBuilder.Entity<UserChatRoom>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.ChatRooms)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserChatRoom>()
            .HasOne(uc => uc.ChatRoom)
            .WithMany(cr => cr.Participants)
            .HasForeignKey(uc => uc.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(cm => cm.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(cm => cm.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
      
        modelBuilder.Entity<Slider>()
           .HasMany(s => s.Items)
           .WithOne(i => i.Slider)
           .HasForeignKey(i => i.SliderId)
           .OnDelete(DeleteBehavior.Cascade);
    }

}
