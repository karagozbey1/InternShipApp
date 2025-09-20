using Business.Interfaces.Internship;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebUI.Models.ViewModels.Student.ViewModels;
using WebUI.Controllers;

namespace WebUI.Controllers.Student
{
    [Authorize]
    public class CalendarController : BaseAppController
    {
        private readonly IInternshipApplicationService _applicationService;
        private readonly IInternshipPeriodService _periodService;
        private readonly IInternshipDocumentService _documentService;

        public CalendarController(
            IInternshipApplicationService applicationService,
            IInternshipPeriodService periodService,
            IInternshipDocumentService documentService)
        {
            _applicationService = applicationService;
            _periodService = periodService;
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var userId = GetCurrentUserId();

            var baseDate = DateTime.Today;
            if (year.HasValue && month.HasValue)
            {
                baseDate = new DateTime(year.Value, month.Value, 1);
            }

            var apps = await _applicationService.GetApplicationsByStudentIdAsync(userId);
            var diary = await _documentService.GetStudentDiaryEntriesAsync(userId);
            var periods = await _periodService.GetStudentCalendarEventsAsync(userId);

            var vm = new CalendarViewModel
            {
                CurrentDate = baseDate,
                MonthTitle = baseDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")),
                Applications = apps.Select(a => new CalendarApplication
                {
                    Title = a.InternshipOffer?.Title ?? "Baþvuru",
                    CompanyName = a.InternshipOffer?.Company?.CompanyName ?? "",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate
                }).ToList(),
                DiaryEntries = diary.Select(d => new CalendarDiaryEntry
                {
                    Date = d.UploadedAt,
                    Title = d.FileName ?? "Belge",
                    Content = d.Description ?? ""
                }).ToList(),
                Periods = periods.Select(p => new CalendarPeriod
                {
                    Title = p.PeriodName ?? "Dönem",
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.PeriodName ?? ""
                }).ToList()
            };

            // Fill marker sets
            var firstDay = new DateTime(baseDate.Year, baseDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            foreach (var p in vm.Periods)
            {
                var start = p.StartDate.Date < firstDay ? firstDay : p.StartDate.Date;
                var end = p.EndDate.Date > lastDay ? lastDay : p.EndDate.Date;
                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    vm.PeriodDays.Add(d.Day);
                }
            }

            foreach (var a in vm.Applications)
            {
                // You may want to mark entire range; currently only start marker
                if (a.StartDate.Year == baseDate.Year && a.StartDate.Month == baseDate.Month)
                    vm.ApplicationDays.Add(a.StartDate.Day);
            }

            foreach (var e in vm.DiaryEntries)
            {
                if (e.Date.Year == baseDate.Year && e.Date.Month == baseDate.Month)
                    vm.DiaryDays.Add(e.Date.Day);
            }

            return View("~/Views/Student/Calendar/Index.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> DayEvents(int year, int month, int day)
        {
            try
            {
                var userId = GetCurrentUserId();
                var date = new DateTime(year, month, day).Date;

                var apps = await _applicationService.GetApplicationsByStudentIdAsync(userId);
                var diary = await _documentService.GetStudentDiaryEntriesAsync(userId);
                var periods = await _periodService.GetStudentCalendarEventsAsync(userId);

                var list = new List<CalendarEvent>();

                // Periods: show only on start or end day to avoid repeating identical info on every day
                foreach (var p in periods)
                {
                    var start = p.StartDate.Date;
                    var end = p.EndDate.Date;
                    if (date == start || date == end)
                    {
                        list.Add(new CalendarEvent
                        {
                            Title = p.PeriodName ?? "Dönem",
                            Description = (date == start && date == end) ? "Tek günlük dönem" : (date == start ? "Dönem baþlangýcý" : "Dönem bitiþi"),
                            StartDate = p.StartDate,
                            EndDate = p.EndDate,
                            Type = "period",
                            Color = "indigo",
                            AllDay = true
                        });
                    }
                }

                // Applications: show only on start date (and optionally end date)
                foreach (var a in apps)
                {
                    var start = a.StartDate.Date;
                    var end = a.EndDate.Date;
                    if (date == start || date == end)
                    {
                        list.Add(new CalendarEvent
                        {
                            Title = a.InternshipOffer?.Title ?? "Staj",
                            Description = date == start ? (a.InternshipOffer?.Company?.CompanyName ?? "Baþlangýç") : "Staj bitiþi",
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            Type = "application",
                            Color = "blue",
                            AllDay = true
                        });
                    }
                }

                // Diary entries: exact day match
                foreach (var d in diary)
                {
                    if (d.UploadedAt.Date == date)
                    {
                        list.Add(new CalendarEvent
                        {
                            Title = d.FileName ?? "Günlük Girdisi",
                            Description = d.Description ?? string.Empty,
                            StartDate = d.UploadedAt,
                            EndDate = d.UploadedAt,
                            Type = "diary",
                            Color = "emerald",
                            AllDay = true
                        });
                    }
                }

                var ordered = list.OrderBy(e => e.StartDate).ThenBy(e => e.Title).Select(e => new
                {
                    type = e.Type,
                    title = e.Title,
                    startDate = e.StartDate,
                    endDate = e.EndDate,
                    description = e.Description,
                    color = e.Color
                }).ToList();

                return Json(new { success = true, events = ordered });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
