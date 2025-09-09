using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using WebUI.Models;
using Business.Interfaces.Order; // IINewsService için
using WebUI.Models.ViewModels.Home;

namespace WebUI.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IINewsService _newsService;
        private readonly IISliderService _sliderService;

        public HomeController(ILogger<HomeController> logger, IINewsService newsService, IISliderService sliderService)
        {
            _logger = logger;
            _newsService = newsService;
            _sliderService = sliderService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeIndexViewModel();
            
            // Get news and announcements
            var allNews = await _newsService.GetAllNewsAsync();
            viewModel.Announcements = allNews.Where(n => n.Category?.Equals("Announcement", StringComparison.OrdinalIgnoreCase) == true)
                                            .OrderByDescending(n => n.PublishDate)
                                            .Take(4)
                                            .Select(n => new AnnouncementItem
                                            {
                                                Title = n.Title,
                                                Content = n.Content,
                                                PublishDate = n.PublishDate
                                            })
                                            .ToList();
            
            viewModel.News = allNews.Where(n => n.Category?.Equals("News", StringComparison.OrdinalIgnoreCase) == true)
                                   .OrderByDescending(n => n.PublishDate)
                                   .Take(4)
                                   .ToList();
            
            // Get active sliders with their items
            viewModel.SliderItems = (await _sliderService.GetActiveSlidersWithItemsAsync())
                .SelectMany(s => s.Items ?? new List<Core.Entity.SliderItem>())
                .ToList();
            
            // Static content that should be made dynamic (can be moved to database later)
            viewModel.ImportantDates = new List<ImportantDate>
            {
                new ImportantDate { Date = DateTime.Parse("2024-02-01"), Description = "Yaz Dönemi Staj Baþvurularý Baþlýyor" },
                new ImportantDate { Date = DateTime.Parse("2024-03-15"), Description = "Staj Baþvuru Son Tarih" },
                new ImportantDate { Date = DateTime.Parse("2024-06-01"), Description = "Yaz Stajlarý Baþlýyor" },
                new ImportantDate { Date = DateTime.Parse("2024-08-30"), Description = "Staj Sunum Tarihleri" }
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

        public IActionResult About() => View();

        public async Task<IActionResult> News()
        {
            var all = await _newsService.GetAllNewsAsync();
            var news = all.Where(n => n.Category?.Equals("News", StringComparison.OrdinalIgnoreCase) == true)
                          .OrderByDescending(n => n.PublishDate)
                          .ToList();
            return View(news);
        }

        public async Task<IActionResult> Announcements()
        {
            var all = await _newsService.GetAllNewsAsync();
            var announcements = all.Where(n => n.Category?.Equals("Announcement", StringComparison.OrdinalIgnoreCase) == true)
                                    .OrderByDescending(n => n.PublishDate)
                                    .ToList();
            return View(announcements);
        }

        public IActionResult Contact() => View();
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
