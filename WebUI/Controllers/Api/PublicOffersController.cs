using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DataAccess.Unit.Interface;

namespace WebUI.Controllers.Api
{
    [ApiController]
    [Route("api/public/offers")] 
    public class PublicOffersController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public PublicOffersController(IUnitOfWork uow){ _uow = uow; }

        // Toggle favorite offer for current user (DB persistent) and keep Session cache in sync
        [HttpPost("{id:int}/favorite")]
        [Authorize(Roles = "Student")] // only students can favorite
        public async Task<IActionResult> ToggleFavorite(int id)
        {
            if (id <= 0) return BadRequest();

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var userKey = $"FavOffers:{userIdClaim.Value}";

            // Read current session cache
            var raw = HttpContext.Session.GetString(userKey) ?? string.Empty;
            var set = new HashSet<int>();
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out var v)) set.Add(v);
            }

            // Persist in DB
            var existing = await _uow.OfferFavorites.GetByUserAndOfferAsync(userId, id);
            var isFavorite = false;
            if (existing != null)
            {
                await _uow.OfferFavorites.RemoveByUserAndOfferAsync(userId, id);
                await _uow.SaveAsync();
                set.Remove(id);
                isFavorite = false;
            }
            else
            {
                await _uow.OfferFavorites.AddAsync(new Core.Entity.OfferFavorite{ StudentId = userId, OfferId = id });
                await _uow.SaveAsync();
                set.Add(id);
                isFavorite = true;
            }

            // Update session cache
            HttpContext.Session.SetString(userKey, string.Join(',', set));

            return new JsonResult(new { success = true, isFavorite });
        }

        [HttpGet("favorites")] 
        [Authorize(Roles = "Student")] 
        public async Task<IActionResult> GetFavorites()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var favs = await _uow.OfferFavorites.GetByUserAsync(userId);
            var list = favs.Select(f => new {
                id = f.OfferId,
                title = f.Offer!.Title,
                companyName = f.Offer!.Company!.CompanyName,
                city = f.Offer!.City,
                field = f.Offer!.Field,
                applicationDeadline = f.Offer!.ApplicationDeadline,
                createdAt = f.CreatedAt
            }).ToList();
            return new JsonResult(list);
        }
    }
}
