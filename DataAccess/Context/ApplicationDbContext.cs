using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<CompanyUser> CompanyUsers { get; set; } = null!;
        public DbSet<InternshipPeriod> InternshipPeriods { get; set; } = null!;
        public DbSet<InternshipOffer> InternshipOffers { get; set; } = null!;
        public DbSet<InternshipApplication> InternshipApplications { get; set; } = null!;
        public DbSet<InternshipApprovalForm> InternshipApprovalForms { get; set; } = null!;
        public DbSet<InternshipDocument> InternshipDocuments { get; set; } = null!;
        public DbSet<InternshipComment> InternshipComments { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;
        public DbSet<ChatRoom> ChatRooms { get; set; } = null!;
        public DbSet<UserChatRoom> UserChatRooms { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Slider> Sliders => Set<Slider>();
        public DbSet<SliderItem> SliderItems => Set<SliderItem>();
        public DbSet<Province> Provinces { get; set; } = null!;
        public DbSet<University> Universities { get; set; } = null!;
        public DbSet<DiaryEntry> DiaryEntries { get; set; } = null!;
        public DbSet<InternshipDiary> InternshipDiaries { get; set; } = null!;
        public DbSet<InternshipPeriodStudent> InternshipPeriodStudents { get; set; } = null!;

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

                entity.HasOne(u => u.University)
                      .WithMany(u => u.Users)
                      .HasForeignKey(u => u.UniversityId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DiaryEntry Entity Configuration
            modelBuilder.Entity<DiaryEntry>(entity =>
            {
                entity.Property(d => d.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(d => d.Content)
                      .HasMaxLength(5000)
                      .IsRequired();

                entity.HasOne(d => d.Student)
                      .WithMany()
                      .HasForeignKey(d => d.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Diary)
                      .WithMany(diary => diary.Entries)
                      .HasForeignKey(d => d.DiaryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // InternshipDiary Entity Configuration
            modelBuilder.Entity<InternshipDiary>(entity =>
            {
                entity.Property(d => d.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(d => d.Description)
                      .HasMaxLength(1000);

                entity.Property(d => d.CompanyName)
                      .HasMaxLength(200);

                entity.Property(d => d.Department)
                      .HasMaxLength(200);

                entity.HasOne(d => d.Student)
                      .WithMany()
                      .HasForeignKey(d => d.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(d => d.Entries)
                      .WithOne(e => e.Diary)
                      .HasForeignKey(e => e.DiaryId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Optional link to the original application (one-to-one-ish)
                entity.HasOne(d => d.Application)
                      .WithMany()
                      .HasForeignKey(d => d.ApplicationId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                entity.HasIndex(d => d.ApplicationId).IsUnique(false);
            });

            // InternshipApprovalForm Entity Configuration
            modelBuilder.Entity<InternshipApprovalForm>(entity =>
            {
                entity.Property(f => f.CompanyName)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(f => f.WorkplaceAddress)
                      .HasMaxLength(500);

                entity.Property(f => f.InternshipCoordinator)
                      .HasMaxLength(100);

                entity.Property(f => f.CoordinatorPhone)
                      .HasMaxLength(20);

                entity.Property(f => f.CoordinatorEmail)
                      .HasMaxLength(100);

                entity.Property(f => f.InternshipDescription)
                      .HasMaxLength(1000);

                entity.Property(f => f.StudentNote)
                      .HasMaxLength(1000);

                entity.Property(f => f.AdvisorNote)
                      .HasMaxLength(1000);

                entity.Property(f => f.RejectionReason)
                      .HasMaxLength(500);

                entity.Property(f => f.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                // İlişkiler
                entity.HasOne(f => f.Application)
                      .WithMany()
                      .HasForeignKey(f => f.ApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Student)
                      .WithMany()
                      .HasForeignKey(f => f.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Advisor)
                      .WithMany()
                      .HasForeignKey(f => f.AdvisorId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Index'ler
                entity.HasIndex(f => f.ApplicationId).IsUnique();
                entity.HasIndex(f => new { f.StudentId, f.Status });
                entity.HasIndex(f => new { f.AdvisorId, f.Status });
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
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure enum conversion for Duration
            modelBuilder.Entity<InternshipOffer>()
                .Property(o => o.Duration)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            // Configure enum conversion for JobType
            modelBuilder.Entity<InternshipOffer>()
                .Property(o => o.JobType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<InternshipApplication>(entity =>
            {
                // Property configurations
                entity.Property(a => a.Status)
                      .HasConversion<string>()
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(a => a.StudentNote)
                      .HasMaxLength(1000);

                // Relationships
                entity.HasOne(a => a.Student)
                      .WithMany(u => u.StudentApplications)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Advisor)
                      .WithMany(u => u.AdvisorApplications)
                      .HasForeignKey(a => a.AdvisorId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false); // AdvisorId nullable olabilir

                entity.HasOne(a => a.InternshipOffer)
                      .WithMany(o => o.InternshipApplications)
                      .HasForeignKey(a => a.InternshipOfferId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(a => new { a.StudentId, a.Status });
                entity.HasIndex(a => new { a.InternshipOfferId, a.Status });
            });

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

            // InternshipDocument konfigürasyonları - DÜZELTİLDİ
            modelBuilder.Entity<InternshipDocument>(entity =>
            {
                // Required alanlar
                entity.Property(d => d.FileName).IsRequired().HasMaxLength(500);
                entity.Property(d => d.FilePath).IsRequired().HasMaxLength(1000);
                entity.Property(d => d.DocumentType).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Title).HasMaxLength(200); // Belge başlık adı
                // Optional alanlar
                entity.Property(d => d.ContentType).HasMaxLength(200);
                entity.Property(d => d.Description).HasMaxLength(1000);
                entity.Property(d => d.Status).HasMaxLength(50);

                // İlişkiler - tümü opsiyonel
                entity.HasOne(d => d.InternshipApplication)
                      .WithMany(a => a.InternshipDocuments)
                      .HasForeignKey(d => d.ApplicationId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                entity.HasOne(d => d.Company)
                      .WithMany()
                      .HasForeignKey(d => d.CompanyId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                entity.HasOne(d => d.Student)
                      .WithMany()
                      .HasForeignKey(d => d.StudentId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Yeni: ApprovalForm ilişkisi (opsiyonel)
                entity.HasOne(d => d.ApprovalForm)
                      .WithMany()
                      .HasForeignKey(d => d.ApprovalFormId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            modelBuilder.Entity<InternshipPeriod>()
               .HasKey(p => p.PeriodId);

            // configure many-to-many via join entity InternshipPeriodStudent
            modelBuilder.Entity<InternshipPeriodStudent>(entity =>
            {
                entity.HasKey(e => new { e.InternshipPeriodId, e.StudentId });

                entity.HasOne(e => e.InternshipPeriod)
                      .WithMany(p => p.PeriodStudents)
                      .HasForeignKey(e => e.InternshipPeriodId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                      .WithMany(u => u.InternshipPeriodStudents)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

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

            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("iller");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("il_id");
                entity.Property(e => e.Name).HasColumnName("il_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            });

            modelBuilder.Entity<University>(entity =>
            {
                entity.ToTable("universiteler");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("uni_id");
                entity.Property(e => e.Name).HasColumnName("uni_name").HasMaxLength(255).IsRequired();
                entity.Property(e => e.ProvinceId).HasColumnName("il_id").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").IsRequired();

                entity.HasOne(e => e.Province)
                      .WithMany(p => p.Universities)
                      .HasForeignKey(e => e.ProvinceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // Normalize DateTime kinds for all tracked entities before saving to database
        private void NormalizeDateTimesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var prop in entry.Properties)
                {
                    if (prop.CurrentValue is DateTime dt)
                    {
                        if (dt.Kind == DateTimeKind.Unspecified)
                        {
                            prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        }
                        else if (dt.Kind == DateTimeKind.Local)
                        {
                            prop.CurrentValue = dt.ToUniversalTime();
                        }
                    }
                    else if (prop.CurrentValue == null)
                    {
                        // nothing to do for nulls
                    }
                }
            }
        }

        public override int SaveChanges()
        {
            NormalizeDateTimesToUtc();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}