using Business.Interfaces.Chat;
using Business.Interfaces.Com;
using Business.Interfaces.Email;
using Business.Interfaces.Internship;
using Business.Interfaces.Order;
using Business.Interfaces.UserRepose;
using Business.Services.Chat;
using Business.Services.Com;
using Business.Services.Email;
using Business.Services.Internship;
using Business.Services.Order;
using Business.Services.UserRepose;
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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session ve Distributed Cache hizmetlerini ekleme.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Kimlik Doðrulama Hizmetini Ekleme.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "InternshipAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.AccessDeniedPath = "/Home/Error";
    });

// UnitOfWork tasarým desenini tanýmlama.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IInternshipOfferRepository, InternshipOfferRepository>();
builder.Services.AddScoped<IInternshipCommentRepository, InternshipCommentRepository>();
builder.Services.AddScoped<IInternshipDocumentRepository, InternshipDocumentRepository>();
builder.Services.AddScoped<IInternshipPeriodRepository, InternshipPeriodRepository>();
builder.Services.AddScoped<IInternshipApplicationRepository, InternshipApplicationRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<ICompanyUserRepository, CompanyUserRepository>();
builder.Services.AddScoped<ISliderRepository, SliderRepository>();
builder.Services.AddScoped<ISliderItemRepository, SliderItemRepository>();


// Services
builder.Services.AddScoped<IICompanyService, ICompanyService>();
builder.Services.AddScoped<IInternshipOfferService, InternshipOfferService>();
builder.Services.AddScoped<IInternshipCommentService, InternshipCommentService>();
builder.Services.AddScoped<IInternshipDocumentService, InternshipDocumentService>();
builder.Services.AddScoped<IInternshipPeriodService, InternshipPeriodService>();
builder.Services.AddScoped<IInternshipApplicationService, InternshipApplicationService>();
builder.Services.AddScoped<IINewsService, INewsService>();
builder.Services.AddScoped<IINotificationService, INotificationService>();
builder.Services.AddScoped<IIUserService, UserService>();
builder.Services.AddScoped<IIChatService, IChatService>();
builder.Services.AddScoped<IICompanyUserService, ICompanyUserService>();
builder.Services.AddScoped<IISliderService, ISliderService>();
builder.Services.AddScoped<IISliderItemService, ISliderItemService>();
// ...

// Doðru interface ve implementation eþleþmesi için satýrý þu þekilde deðiþtirin:
builder.Services.AddScoped<IIActivationEmailService, MailKitActivationEmailService>();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();