using DataAccess.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> TestUniversity()
        {
            try
            {
                // Test 1: Basic university query
                var universities = await _context.Universities.Take(5).ToListAsync();
                ViewBag.UniversityCount = universities.Count;
                ViewBag.Universities = universities.Select(u => u.Name).ToList();

                // Test 2: Test province relationship
                var universitiesWithProvince = await _context.Universities
                    .Include(u => u.Province)
                    .Take(5)
                    .ToListAsync();
                ViewBag.UniversitiesWithProvince = universitiesWithProvince
                    .Select(u => $"{u.Name} - {u.Province?.Name}")
                    .ToList();

                // Test 3: Test User-University relationship
                var usersWithUniversity = await _context.Users
                    .Include(u => u.University)
                    .Where(u => u.UniversityId != null)
                    .Take(5)
                    .ToListAsync();
                ViewBag.UsersWithUniversity = usersWithUniversity
                    .Select(u => $"{u.FullName} - {u.University?.Name}")
                    .ToList();

                ViewBag.Success = true;
                ViewBag.Message = "All tests passed!";
            }
            catch (Exception ex)
            {
                ViewBag.Success = false;
                ViewBag.Message = ex.Message;
                ViewBag.StackTrace = ex.StackTrace;
            }

            return View();
        }
    }
}