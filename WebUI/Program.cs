using Business.Education;
using Business.Interfaces;
using Business.Interfaces.Chat;
using Business.Interfaces.Com;
using Business.Interfaces.Email;
using Business.Interfaces.Internship;
using Business.Interfaces.Order;
using Business.Interfaces.UserRepose;
using Business.Services;
using Business.Services.Chat;
using Business.Services.Com;
using Business.Services.Interfaces;
using Business.Services.Internship;
using Business.Services.Order;
using Business.Services.UserRepose;
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
using DataAccess.Unit.Repose;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebUI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Global exception filter ekleme
    options.Filters.Add<AuthExceptionFilter>();
});

// Runtime compilation support
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
    {
        // PostgreSQL için timestamp handling optimize et
        npgsqlOptions.EnableRetryOnFailure();
    });
    
    // Development'ta detailed logging
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// Session ve Distributed Cache hizmetlerini ekleme.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login"; // Authentication gerektiren sayfalara gidildiðinde
        o.AccessDeniedPath = "/Account/Login"; // Yetki sorunu olduðunda
        o.Cookie.Name = "InternshipAuth";
        o.ExpireTimeSpan = TimeSpan.FromHours(8); // Default session süre
        o.SlidingExpiration = false; // RememberMe ile çakýþmasýn diye false
        o.ReturnUrlParameter = "returnUrl"; // Login sonrasý nereye dönüleceði
        o.Cookie.HttpOnly = true; // XSS güvenliði
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS güvenliði
        o.Cookie.SameSite = SameSiteMode.Lax; // CSRF güvenliði
    });

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IInternshipOfferRepository, InternshipOfferRepository>();
builder.Services.AddScoped<IInternshipCommentRepository, InternshipCommentRepository>();
builder.Services.AddScoped<IInternshipDocumentRepository, InternshipDocumentRepository>();
builder.Services.AddScoped<IInternshipPeriodRepository, InternshipPeriodRepository>();
builder.Services.AddScoped<IInternshipApplicationRepository, InternshipApplicationRepository>();
builder.Services.AddScoped<IInternshipApprovalRepository, InternshipApprovalRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<ICompanyUserRepository, CompanyUserRepository>();
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<IDiaryRepository, DiaryRepository>();
builder.Services.AddScoped<IInternshipDiaryRepository, InternshipDiaryRepository>();
builder.Services.AddScoped<DataAccess.Interfaces.Internship.IOfferFavoriteRepository, DataAccess.Repositories.Internship.OfferFavoriteRepository>();

// Services
builder.Services.AddScoped<IICompanyService, ICompanyService>();
builder.Services.AddScoped<IInternshipOfferService, InternshipOfferService>();
builder.Services.AddScoped<IInternshipCommentService, InternshipCommentService>();
builder.Services.AddScoped<IInternshipDocumentService, InternshipDocumentService>();
builder.Services.AddScoped<IInternshipPeriodService, InternshipPeriodService>();
builder.Services.AddScoped<IInternshipApplicationService, InternshipApplicationService>();
builder.Services.AddScoped<IInternshipApprovalService, InternshipApprovalService>();
builder.Services.AddScoped<IINotificationService, INotificationService>();
builder.Services.AddScoped<IIUserService, UserService>();
builder.Services.AddScoped<IIChatService, IChatService>();
builder.Services.AddScoped<IICompanyUserService, ICompanyUserService>();
builder.Services.AddScoped<IIProvinceService, ProvinceService>();
builder.Services.AddScoped<IIUniversityService, UniversityService>();
builder.Services.AddScoped<IIActivationEmailService, MailKitActivationEmailService>();
builder.Services.AddScoped<IDiaryService, DiaryService>();
builder.Services.AddScoped<IInternshipDiaryService, InternshipDiaryService>();
// builder.Services.AddScoped<ISiteContentService, SiteContentService>(); // Removed because About/Contact pages removed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Attribute-routed API controller'larý etkinleþtir
app.MapControllers();

// Student controller'lar için özel route (Student namespace'i için)
app.MapControllerRoute(
    name: "studentArea",
    pattern: "Student/{controller}/{action=Index}/{id?}",
    defaults: new { area = "" }
);

// Company controller'lar için özel route
app.MapControllerRoute(
    name: "company",
    pattern: "Company/{controller}/{action=Index}/{id?}",
    defaults: new { area = "" }
);

// Advisor controller'lar için özel route
app.MapControllerRoute(
    name: "advisor",
    pattern: "Advisor/{controller}/{action=Index}/{id?}",
    defaults: new { area = "" }
);

// API routes için özel pattern
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Index}/{id?}"
);

// Ana sayfa için default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
