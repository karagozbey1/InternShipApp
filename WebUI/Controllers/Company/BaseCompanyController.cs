using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Business.Interfaces.Com;
using Business.Interfaces.Internship;
using Business.Interfaces.UserRepose;
using Business.Interfaces.Order;
using Business.Interfaces.Chat;
using Core.Entity;
using System.Security.Claims;
using WebUI.Models.Company;

namespace WebUI.Controllers.Company
{
    // Temel þirket kontrolcü sýnýfý
    [Authorize(Roles = "Company")]
    public abstract class BaseCompanyController : Controller
    {
        // Servisler
        protected readonly IICompanyService _companyService;
        protected readonly IInternshipOfferService _offerService;
        protected readonly IInternshipApplicationService _applicationService;
        protected readonly IIUserService _userService;
        protected readonly IINotificationService _notificationService;
        protected readonly IIChatService _chatService;

        // Kurucu metod
        protected BaseCompanyController(
            IICompanyService companyService,
            IInternshipOfferService offerService,
            IInternshipApplicationService applicationService,
            IIUserService userService,
            IINotificationService notificationService,
            IIChatService chatService)
        {
            _companyService = companyService;
            _offerService = offerService;
            _applicationService = applicationService;
            _userService = userService;
            _notificationService = notificationService;
            _chatService = chatService;
        }

        // Mevcut kullanýcý ID'sini getirir
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // Mevcut þirket ID'sini getirir
        protected async Task<int> GetCurrentCompanyIdAsync()
        {
            var userId = GetCurrentUserId();
            var company = await _companyService.GetCompanyByUserIdAsync(userId);
            return company?.CompanyId ?? 0;
        }

        // Son baþvurularý görünüm modeline dönüþtürür
        protected RecentApplicationViewModel MapToRecentApplicationViewModel(InternshipApplication application)
        {
            return new RecentApplicationViewModel
            {
                Id = application.Id,
                StudentName = application.Student?.FullName ?? "Bilinmeyen",
                JobTitle = application.InternshipOffer?.Title ?? "Bilinmeyen Pozisyon",
                ApplicationDate = application.CreatedAt,
                Status = MapToViewModelStatus(application.Status)
            };
        }

        // Aktif stajyerleri görünüm modeline dönüþtürür
        protected ActiveInternViewModel MapToActiveInternViewModel(InternshipApplication application)
        {
            var totalDays = CalculateWorkingDays(application.StartDate, application.EndDate);
            var completedDays = CalculateWorkingDays(application.StartDate, DateTime.Now < application.EndDate ? DateTime.Now : application.EndDate);
            return new ActiveInternViewModel
            {
                Id = application.Id,
                StudentId = application.StudentId,
                StudentName = application.Student?.FullName ?? "Bilinmeyen",
                Position = application.InternshipOffer?.Field ?? "Bilinmeyen Pozisyon",
                StartDate = application.StartDate,
                EndDate = application.EndDate,
                CompletedDays = Math.Max(0, completedDays),
                TotalDays = totalDays
            };
        }

        // Durumu görünüm modeline dönüþtürür
        protected ApplicationStatus MapToViewModelStatus(InternshipApplicationStatus coreStatus) => coreStatus switch
        {
            InternshipApplicationStatus.Pending => ApplicationStatus.Pending,
            InternshipApplicationStatus.Approved => ApplicationStatus.Approved,
            InternshipApplicationStatus.Rejected => ApplicationStatus.Rejected,
            _ => ApplicationStatus.Pending
        };

        // Çalýþma günlerini hesaplar
        private int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate) return 0;

            int workingDays = 0;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
            }
            return workingDays;
        }
    }
}
