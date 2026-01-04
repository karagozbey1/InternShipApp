using DataAccess.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Core.Entity;

namespace InternshipTrackingSystem.IntegrationTests.Infrastructure;

/// <summary>
/// Integration testler icin ozel WebApplicationFactory
/// SQLite in-memory database kullaniyor (izole ve hizli)
/// Test auth handler ile authentication simule ediyor
/// </summary>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    // SQLite connection - factory omru boyunca acik tutulmali
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Mevcut DbContext ve ilgili tum servisleri kaldir
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextServiceDescriptor != null)
                services.Remove(dbContextServiceDescriptor);

            // Npgsql/PostgreSQL ile ilgili servisleri kaldir
            var npgsqlDescriptors = services.Where(
                d => d.ServiceType.FullName?.Contains("Npgsql") == true ||
                     d.ImplementationType?.FullName?.Contains("Npgsql") == true ||
                     d.ServiceType.FullName?.Contains("DbContext") == true).ToList();
            foreach (var d in npgsqlDescriptors)
                services.Remove(d);

            // SQLite in-memory connection (tek connection, acik tutulacak)
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            // SQLite ile yeni DbContext ekle
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseSqlite(_connection);
            });

            // Test Authentication handler ekle (mevcut auth'u override et)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>("TestScheme", options => { });
        });

        // Configure sonrasi DB'yi seed et
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                db.Database.EnsureCreated();
                SeedTestData(db);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetService<ILogger<CustomWebApplicationFactory<TProgram>>>();
                logger?.LogError(ex, "Test veritabani seed sirasinda hata olustu: {Message}", ex.Message);
            }
        });

        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// Test verileri ekler - gercekci senaryolar icin minimal ama yeterli veri
    /// </summary>
    private static void SeedTestData(ApplicationDbContext db)
    {
        // Test kullanicilari
        if (!db.Users.Any())
        {
            var studentUser = new User
            {
                FullName = "Test Student",
                Email = "student@test.com",
                PasswordHash = Business.OrderMethod.PasswordSecurity.HashPassword("Test123!"),
                Role = UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                StudentNumber = "2024001",
                Department = "Bilgisayar Muhendisligi",
                Grade = 3
            };

            var advisorUser = new User
            {
                FullName = "Test Advisor",
                Email = "advisor@test.com",
                PasswordHash = Business.OrderMethod.PasswordSecurity.HashPassword("Test123!"),
                Role = UserRole.Advisor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Title = "Dr.",
                Department = "Bilgisayar Muhendisligi"
            };

            var companyUser = new User
            {
                FullName = "Test Company User",
                Email = "company@test.com",
                PasswordHash = Business.OrderMethod.PasswordSecurity.HashPassword("Test123!"),
                Role = UserRole.Company,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.AddRange(studentUser, advisorUser, companyUser);
            db.SaveChanges();
        }

        // Test sirketi
        if (!db.Companies.Any())
        {
            var company = new Company
            {
                CompanyName = "Test Teknoloji A.S.",
                Sector = "Bilisim",
                Phone = "02121234567",
                TaxNumber = "1234567890",
                Approved = true,
                Email = "info@testtech.com",
                Address = "Istanbul, Turkiye"
            };
            db.Companies.Add(company);
            db.SaveChanges();

            // Company'nin Id'sini al
            var companyId = db.Companies.First().CompanyId;
            var companyUserId = db.Users.First(u => u.Role == UserRole.Company).Id;

            // CompanyUser iliskisi
            var companyUserLink = new CompanyUser
            {
                CompanyId = companyId,
                UserId = companyUserId,
                RoleInCompany = "Admin"
            };
            db.CompanyUsers.Add(companyUserLink);
            db.SaveChanges();
        }

        // Test staj donemi
        if (!db.InternshipPeriods.Any())
        {
            var period = new InternshipPeriod
            {
                PeriodName = "2025 Yaz Donemi",
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(90)
            };
            db.InternshipPeriods.Add(period);
            db.SaveChanges();
        }

        // Test staj ilani
        if (!db.InternshipOffers.Any())
        {
            var companyId = db.Companies.First().CompanyId;
            var periodId = db.InternshipPeriods.First().PeriodId;

            var offer = new InternshipOffer
            {
                Title = "Yazilim Gelistirici Stajyeri",
                Description = "Full-stack web gelistirme staji",
                Requirements = ".NET, React, SQL bilgisi",
                CompanyId = companyId,
                PeriodId = periodId,
                IsActive = true,
                City = "Istanbul",
                Field = "Yazilim Gelistirme",
                Duration = OfferDurationType.Kisa,
                JobType = OfferJobType.Internship,
                CreatedAt = DateTime.UtcNow,
                AvailablePositions = 5,
                ApplicationDeadline = DateTime.UtcNow.AddDays(25)
            };
            db.InternshipOffers.Add(offer);
            db.SaveChanges();
        }

        // Test basvuru
        if (!db.InternshipApplications.Any())
        {
            var studentId = db.Users.First(u => u.Role == UserRole.Student).Id;
            var offerId = db.InternshipOffers.First().Id;

            var application = new InternshipApplication
            {
                StudentId = studentId,
                InternshipOfferId = offerId,
                Status = InternshipApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                StartDate = DateTime.UtcNow.AddDays(30),
                EndDate = DateTime.UtcNow.AddDays(60),
                StudentNote = "Staj basvurusu",
                UpdatedAt = DateTime.UtcNow
            };
            db.InternshipApplications.Add(application);
            db.SaveChanges();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
