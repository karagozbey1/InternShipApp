using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using WebUI.Models;
using WebUI.Models.ViewModels.Home;
using Business.Interfaces.Internship;
using Business.Interfaces.Com;
using Business.Services.Interfaces; // for IIProvinceService

namespace WebUI.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInternshipOfferService _offerService;
        private readonly IICompanyService _companyService;
        private readonly IIProvinceService _provinceService; // new

        public HomeController(ILogger<HomeController> logger,
            IInternshipOfferService offerService, IICompanyService companyService, IIProvinceService provinceService)
        {
            _logger = logger;
            _offerService = offerService;
            _companyService = companyService;
            _provinceService = provinceService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeIndexViewModel();
            
            // Get news and announcements (news removed)
            viewModel.Announcements = new List<AnnouncementItem>();
            
            // Static content that should be made dynamic (can be moved to database later)
            viewModel.ImportantDates = new List<ImportantDate>
            {
                new ImportantDate { Date = DateTime.Parse("2025-02-01"), Description = "Yaz Dönemi Staj Baþvurularý Baþlýyor" },
                new ImportantDate { Date = DateTime.Parse("2025-03-15"), Description = "Staj Baþvuru Son Tarihi" },
                new ImportantDate { Date = DateTime.Parse("2025-06-01"), Description = "Yaz Stajlarý Baþlýyor" },
                new ImportantDate { Date = DateTime.Parse("2025-08-30"), Description = "Staj Sunum Tarihleri" }
            };
            
            viewModel.ContactInfo = new ContactInfo
            {
                Address = "Üniversite Caddesi No:123, Ýstanbul",
                Email = "staj@universite.edu.tr",
                Phone = "0212 555 01 23",
                WorkingHours = "Pazartesi-Cuma 09:00-17:00",
                Website = "www.universite.edu.tr"
            };
            
            return View(viewModel);
        }

        // New solutions landing (Herkes için çözüm) - acts as hub to login flows
        [HttpGet]
        public IActionResult Solutions() => View();

        // Duyurular / Ýlanlar (company active offers) with filtering + pagination
        public async Task<IActionResult> Announcements(string? q, string? city, string? field, string? company, string sort = "new", int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 50 ? 10 : pageSize;
            var offers = await _offerService.GetAllActiveOffersAsync();
            var now = DateTime.UtcNow;
            offers = offers.Where(o => o.IsActive && (!o.ApplicationDeadline.HasValue || o.ApplicationDeadline.Value >= now)).ToList();
            var companyIds = offers.Where(o => o.CompanyId > 0).Select(o => o.CompanyId).Distinct().ToList();
            var companyMap = new Dictionary<int, Core.Entity.Company>();
            foreach (var cid in companyIds)
            {
                try { var comp = await _companyService.GetCompanyByIdAsync(cid); if (comp != null) companyMap[cid] = comp; } catch { }
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                offers = offers.Where(o => (o.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                                        || (o.Field?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                                        || (companyMap.TryGetValue(o.CompanyId, out var c) && (c.CompanyName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                                        || (o.City?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(city))
                offers = offers.Where(o => string.Equals(o.City, city, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(field))
                offers = offers.Where(o => string.Equals(o.Field, field, StringComparison.OrdinalIgnoreCase)).ToList();
            // company filter intentionally removed per request
            offers = sort switch
            {
                "old" => offers.OrderBy(o => o.CreatedAt).ToList(),
                "title" => offers.OrderBy(o => o.Title).ToList(),
                _ => offers.OrderByDescending(o => o.CreatedAt).ToList()
            };
            var total = offers.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;
            var pageItems = offers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var list = pageItems.Select(o => new PublicOfferListItem
            {
                Id = o.Id,
                Title = o.Title ?? string.Empty,
                Description = o.Description ?? string.Empty,
                Requirements = o.Requirements ?? string.Empty,
                PublishDate = o.CreatedAt,
                CompanyName = companyMap.TryGetValue(o.CompanyId, out var c) ? c.CompanyName ?? string.Empty : string.Empty,
                City = o.City ?? string.Empty,
                Field = o.Field ?? string.Empty,
                Position = o.Field ?? string.Empty,
                Quota = o.AvailablePositions,
                WorkType = o.JobType.ToString(),
                Salary = o.MonthlySalary.HasValue ? ($"{o.MonthlySalary:0} {o.MonthlySalaryCurrency}") : string.Empty,
                Phone = companyMap.TryGetValue(o.CompanyId, out var c2) ? c2.Phone ?? string.Empty : string.Empty,
                Address = companyMap.TryGetValue(o.CompanyId, out var c3) ? c3.Address ?? string.Empty : string.Empty,
                Website = companyMap.TryGetValue(o.CompanyId, out var c4) ? c4.Website ?? string.Empty : string.Empty,
                IsFavorite = false,
                IsActive = o.IsActive,
                ApplicationDeadline = o.ApplicationDeadline,
                CompanyDescription = companyMap.TryGetValue(o.CompanyId, out var c5) ? c5.Description : null,
                EmployeeCount = companyMap.TryGetValue(o.CompanyId, out var c6) ? c6.EmployeeCount : null,
                EstablishedDate = companyMap.TryGetValue(o.CompanyId, out var c7) ? c7.EstablishedDate : null
            }).ToList();

            // dynamic cities from Province table
            var provinces = await _provinceService.GetAllAsync();
            var provinceNames = provinces.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();

            var vm = new AnnouncementOffersViewModel
            {
                Query = q ?? string.Empty,
                City = city ?? string.Empty,
                Field = field ?? string.Empty,
                Company = string.Empty, // no company filter
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Offers = list,
                DistinctCities = provinceNames,
                DistinctFields = offers.Select(o => o.Field).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList(),
                DistinctCompanies = new List<string>() // not used anymore
            };
            return View(vm);
        }

        // List + page build reused for AJAX + normal
        private async Task<AnnouncementOffersViewModel> BuildOffersVm(string? q, string? city, string? field, string? company, string sort, int page, int pageSize)
        {
            page = page < 1 ? 1 : page; pageSize = pageSize is < 5 or > 50 ? 10 : pageSize;
            var offers = await _offerService.GetAllActiveOffersAsync();
            var now = DateTime.UtcNow;
            offers = offers.Where(o => o.IsActive && (!o.ApplicationDeadline.HasValue || o.ApplicationDeadline.Value >= now)).ToList();
            var companyIds = offers.Where(o => o.CompanyId > 0).Select(o => o.CompanyId).Distinct().ToList();
            var companyMap = new Dictionary<int, Core.Entity.Company>();
            foreach (var cid in companyIds) { try { var comp = await _companyService.GetCompanyByIdAsync(cid); if (comp != null) companyMap[cid] = comp; } catch { } }
            if (!string.IsNullOrWhiteSpace(q)) { var term = q.Trim(); offers = offers.Where(o => (o.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || (o.Field?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || (companyMap.TryGetValue(o.CompanyId, out var c) && (c.CompanyName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)) || (o.City?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToList(); }
            if (!string.IsNullOrWhiteSpace(city)) offers = offers.Where(o => string.Equals(o.City, city, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(field)) offers = offers.Where(o => string.Equals(o.Field, field, StringComparison.OrdinalIgnoreCase)).ToList();
            // company filter removed intentionally
            offers = sort switch { "old" => offers.OrderBy(o => o.CreatedAt).ToList(), "title" => offers.OrderBy(o => o.Title).ToList(), _ => offers.OrderByDescending(o => o.CreatedAt).ToList() };
            var total = offers.Count; var totalPages = (int)Math.Ceiling(total / (double)pageSize); if (page > totalPages && totalPages > 0) page = totalPages;
            var favSet = GetFavoriteOfferIds();
            var pageItems = offers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var list = pageItems.Select(o => new PublicOfferListItem
            {
                Id = o.Id,
                Title = o.Title ?? string.Empty,
                Description = o.Description ?? string.Empty,
                Requirements = o.Requirements ?? string.Empty,
                PublishDate = o.CreatedAt,
                CompanyName = companyMap.TryGetValue(o.CompanyId, out var c) ? c.CompanyName ?? string.Empty : string.Empty,
                City = o.City ?? string.Empty,
                Field = o.Field ?? string.Empty,
                Position = o.Field ?? string.Empty,
                Quota = o.AvailablePositions,
                WorkType = o.JobType.ToString(),
                Salary = o.MonthlySalary.HasValue ? ($"{o.MonthlySalary:0} {o.MonthlySalaryCurrency}") : string.Empty,
                Phone = companyMap.TryGetValue(o.CompanyId, out var c2) ? c2.Phone ?? string.Empty : string.Empty,
                Address = companyMap.TryGetValue(o.CompanyId, out var c3) ? c3.Address ?? string.Empty : string.Empty,
                Website = companyMap.TryGetValue(o.CompanyId, out var c4) ? c4.Website ?? string.Empty : string.Empty,
                IsFavorite = favSet.Contains(o.Id),
                IsActive = o.IsActive,
                ApplicationDeadline = o.ApplicationDeadline,
                CompanyDescription = companyMap.TryGetValue(o.CompanyId, out var c5) ? c5.Description : null,
                EmployeeCount = companyMap.TryGetValue(o.CompanyId, out var c6) ? c6.EmployeeCount : null,
                EstablishedDate = companyMap.TryGetValue(o.CompanyId, out var c7) ? c7.EstablishedDate : null
            }).ToList();
            var provinces = await _provinceService.GetAllAsync();
            var provinceNames = provinces.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
            return new AnnouncementOffersViewModel
            {
                Query = q ?? string.Empty,
                City = city ?? string.Empty,
                Field = field ?? string.Empty,
                Company = string.Empty,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Offers = list,
                DistinctCities = provinceNames,
                DistinctFields = offers.Select(o => o.Field).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList(),
                DistinctCompanies = new List<string>()
            };
        }

        // AJAX endpoint for pagination / filtering
        [HttpGet]
        [Route("api/public/offers")]
        public async Task<IActionResult> GetOffersApi(string? q, string? city, string? field, string? company, string sort = "new", int page = 1, int pageSize = 10)
        {
            var vm = await BuildOffersVm(q, city, field, company, sort, page, pageSize);
            var result = new
            {
                page = vm.Page,
                totalPages = vm.TotalPages,
                totalCount = vm.TotalCount,
                items = vm.Offers.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    companyName = o.CompanyName,
                    publishDate = o.PublishDate,
                    city = o.City,
                    field = o.Field,
                    position = o.Position,
                    salary = o.Salary,
                    workType = (o.WorkType?.Equals("Internship", StringComparison.OrdinalIgnoreCase) ?? false) ? "Staj" : o.WorkType,
                    quota = o.Quota,
                    isFavorite = o.IsFavorite,
                    description = o.Description,
                    requirements = o.Requirements,
                    phone = o.Phone,
                    address = o.Address,
                    website = o.Website,
                    isActive = o.IsActive,
                    applicationDeadline = o.ApplicationDeadline,
                    companyDescription = o.CompanyDescription,
                    employeeCount = o.EmployeeCount,
                    establishedDate = o.EstablishedDate
                })
            };
            return Json(result);
        }

        // Favorite toggle (cookie based demo). Real implementation should persist per user.
        [Authorize(Roles = "Student")]
        [HttpPost]
        [Route("api/public/offers/{id}/favorite")]        
        public IActionResult ToggleFavorite(int id)
        {
            var favs = GetFavoriteOfferIds();
            if (favs.Contains(id)) favs.Remove(id); else favs.Add(id);
            SaveFavoriteOfferIds(favs);
            return Json(new { success = true, isFavorite = favs.Contains(id) });
        }

        private HashSet<int> GetFavoriteOfferIds()
        {
            var cookie = Request.Cookies["fav_offers"];
            if (string.IsNullOrWhiteSpace(cookie)) return new HashSet<int>();
            try { return new HashSet<int>(cookie.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse)); } catch { return new HashSet<int>(); }
        }
        private void SaveFavoriteOfferIds(HashSet<int> set)
        {
            Response.Cookies.Append("fav_offers", string.Join('-', set), new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), HttpOnly = false, IsEssential = true });
        }

        public IActionResult Privacy() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
