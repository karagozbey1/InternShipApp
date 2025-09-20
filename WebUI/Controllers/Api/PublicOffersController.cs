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
            var userKey = $"favOffers:{userId}"; // Changed to match localStorage key format

            System.Diagnostics.Debug.WriteLine($"ToggleFavorite called: userId={userId}, offerId={id}");

            // Read current session cache
            var raw = HttpContext.Session.GetString(userKey) ?? string.Empty;
            System.Diagnostics.Debug.WriteLine($"Current session data: {raw}");
            
            var set = new HashSet<int>();
            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out var v)) set.Add(v);
            }
            System.Diagnostics.Debug.WriteLine($"Parsed session set: {string.Join(",", set)}");

            try
            {
                // Persist in DB
                var existing = await _uow.OfferFavorites.GetByUserAndOfferAsync(userId, id);
                var isFavorite = false;
                
                if (existing != null)
                {
                    System.Diagnostics.Debug.WriteLine("Removing from favorites (DB and session)");
                    await _uow.OfferFavorites.RemoveByUserAndOfferAsync(userId, id);
                    await _uow.SaveAsync();
                    set.Remove(id);
                    isFavorite = false;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Adding to favorites (DB and session)");
                    await _uow.OfferFavorites.AddAsync(new Core.Entity.OfferFavorite{ StudentId = userId, OfferId = id });
                    await _uow.SaveAsync();
                    set.Add(id);
                    isFavorite = true;
                }

                // Update session cache
                var newSessionData = string.Join(',', set);
                HttpContext.Session.SetString(userKey, newSessionData);
                System.Diagnostics.Debug.WriteLine($"Updated session data: {newSessionData}");

                return new JsonResult(new { success = true, isFavorite });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favorite toggle DB error: {ex.Message}");
                
                // Session cache ile çalýþ
                bool isFavorite;
                if (set.Contains(id))
                {
                    System.Diagnostics.Debug.WriteLine("Fallback: Removing from session only");
                    set.Remove(id);
                    isFavorite = false;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Fallback: Adding to session only");
                    set.Add(id);
                    isFavorite = true;
                }
                
                var fallbackSessionData = string.Join(',', set);
                HttpContext.Session.SetString(userKey, fallbackSessionData);
                System.Diagnostics.Debug.WriteLine($"Fallback session data: {fallbackSessionData}");
                
                return new JsonResult(new { success = true, isFavorite });
            }
        }

        [HttpPost("sync-favorites")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SyncFavorites([FromBody] SyncFavoritesRequest request)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            System.Diagnostics.Debug.WriteLine($"Syncing favorites for user: {userId}, IDs: {string.Join(",", request.FavoriteIds ?? new List<int>())}");
            
            if (request.FavoriteIds == null || !request.FavoriteIds.Any())
            {
                return new JsonResult(new { success = true, message = "No favorites to sync" });
            }
            
            try
            {
                var syncCount = 0;
                
                foreach (var offerId in request.FavoriteIds)
                {
                    var existing = await _uow.OfferFavorites.GetByUserAndOfferAsync(userId, offerId);
                    if (existing == null)
                    {
                        await _uow.OfferFavorites.AddAsync(new Core.Entity.OfferFavorite 
                        { 
                            StudentId = userId, 
                            OfferId = offerId,
                            CreatedAt = DateTime.UtcNow
                        });
                        syncCount++;
                    }
                }
                
                if (syncCount > 0)
                {
                    await _uow.SaveAsync();
                    System.Diagnostics.Debug.WriteLine($"Successfully synced {syncCount} new favorites to DB");
                }
                
                // Also update session
                var userKey = $"favOffers:{userId}";
                var sessionData = string.Join(',', request.FavoriteIds);
                HttpContext.Session.SetString(userKey, sessionData);
                
                return new JsonResult(new { success = true, syncedCount = syncCount });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Sync favorites error: {ex.Message}");
                
                // At least update session
                try
                {
                    var userKey = $"favOffers:{userId}";
                    var sessionData = string.Join(',', request.FavoriteIds);
                    HttpContext.Session.SetString(userKey, sessionData);
                }
                catch (Exception sessionEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Session update error: {sessionEx.Message}");
                }
                
                return new JsonResult(new { success = false, error = "Sync failed but session updated" });
            }
        }

        [HttpGet("favorites")] 
        [Authorize(Roles = "Student")] 
        public async Task<IActionResult> GetFavorites()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            System.Diagnostics.Debug.WriteLine($"Getting favorites for user: {userId}");
            
            try
            {
                // Always try DB first - this ensures persistence across sessions
                var favs = await _uow.OfferFavorites.GetByUserAsync(userId);
                System.Diagnostics.Debug.WriteLine($"Found {favs?.Count ?? 0} favorites from DB");
                
                if (favs != null && favs.Count > 0)
                {
                    var list = favs.Select(f => new {
                        id = f.OfferId,
                        title = f.Offer?.Title ?? "Ýsimsiz Ýlan",
                        companyName = f.Offer?.Company?.CompanyName ?? "Bilinmeyen Þirket",
                        city = f.Offer?.City ?? "",
                        field = f.Offer?.Field ?? "",
                        applicationDeadline = f.Offer?.ApplicationDeadline,
                        createdAt = f.CreatedAt
                    }).ToList();
                    
                    // Also sync this data to session for faster access
                    var userKey = $"favOffers:{userId}";
                    var dbFavoriteIds = string.Join(',', favs.Select(f => f.OfferId));
                    HttpContext.Session.SetString(userKey, dbFavoriteIds);
                    System.Diagnostics.Debug.WriteLine($"Synced {list.Count} favorites to session: {dbFavoriteIds}");
                    
                    return new JsonResult(list);
                }
                
                // No favorites in DB, check session as fallback
                System.Diagnostics.Debug.WriteLine("No favorites in DB, checking session...");
                var userKey2 = $"favOffers:{userId}";
                var raw = HttpContext.Session.GetString(userKey2) ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"Session data: {raw}");
                
                if (string.IsNullOrEmpty(raw))
                {
                    System.Diagnostics.Debug.WriteLine("No session data either, returning empty list");
                    return new JsonResult(new List<object>());
                }
                
                var sessionFavoriteIds = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => int.TryParse(s, out var v) ? v : 0)
                                    .Where(v => v > 0)
                                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Found {sessionFavoriteIds.Count} favorite IDs in session: {string.Join(",", sessionFavoriteIds)}");

                if (sessionFavoriteIds.Count > 0)
                {
                    // Try to save session data to DB for persistence
                    try
                    {
                        foreach (var offerId in sessionFavoriteIds)
                        {
                            var existing = await _uow.OfferFavorites.GetByUserAndOfferAsync(userId, offerId);
                            if (existing == null)
                            {
                                await _uow.OfferFavorites.AddAsync(new Core.Entity.OfferFavorite 
                                { 
                                    StudentId = userId, 
                                    OfferId = offerId,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                        await _uow.SaveAsync();
                        System.Diagnostics.Debug.WriteLine("Successfully synced session favorites to DB");
                    }
                    catch (Exception syncEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to sync session to DB: {syncEx.Message}");
                    }

                    // Gerçek offer detaylarýný getir
                    var allActiveOffers = await _uow.InternshipOffers.GetAllActiveOffersAsync();
                    System.Diagnostics.Debug.WriteLine($"Total active offers: {allActiveOffers?.Count ?? 0}");
                    
                    if (allActiveOffers != null)
                    {
                        var favoriteOffers = allActiveOffers
                            .Where(o => sessionFavoriteIds.Contains(o.Id))
                            .Select(o => new {
                                id = o.Id,
                                title = o.Title ?? "Ýsimsiz Ýlan",
                                companyName = o.Company?.CompanyName ?? "Bilinmeyen Þirket",
                                city = o.City ?? "",
                                field = o.Field ?? "",
                                applicationDeadline = o.ApplicationDeadline,
                                createdAt = o.CreatedAt
                            }).ToList();

                        System.Diagnostics.Debug.WriteLine($"Returning {sessionFavoriteIds.Count} session favorites with offer details");
                        return new JsonResult(favoriteOffers);
                    }
                }
                
                // Final fallback - return empty list
                System.Diagnostics.Debug.WriteLine("Final fallback: returning empty list");
                return new JsonResult(new List<object>());
            }
            catch (Exception ex)
            {
                // Database error - fallback to session and try to recover
                System.Diagnostics.Debug.WriteLine($"Get favorites DB error: {ex.Message}");
                
                try
                {
                    // Session'dan favori listesini al
                    var userKey = $"favOffers:{userId}";
                    var raw = HttpContext.Session.GetString(userKey) ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"Fallback - Session data: {raw}");
                    
                    var fallbackFavoriteIds = raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => int.TryParse(s, out var v) ? v : 0)
                                        .Where(v => v > 0)
                                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Fallback - Found {fallbackFavoriteIds.Count} IDs");

                    if (fallbackFavoriteIds.Count > 0)
                    {
                        // Gerçek offer detaylarýný getir
                        var allActiveOffers = await _uow.InternshipOffers.GetAllActiveOffersAsync();
                        if (allActiveOffers != null)
                        {
                            var favoriteOffers = allActiveOffers
                                .Where(o => fallbackFavoriteIds.Contains(o.Id))
                                .Select(o => new {
                                    id = o.Id,
                                    title = o.Title ?? "Ýsimsiz Ýlan",
                                    companyName = o.Company?.CompanyName ?? "Bilinmeyen Þirket",
                                    city = o.City ?? "",
                                    field = o.Field ?? "",
                                    applicationDeadline = o.ApplicationDeadline,
                                    createdAt = o.CreatedAt
                                }).ToList();

                            System.Diagnostics.Debug.WriteLine($"Fallback - Returning {favoriteOffers.Count} offers");
                            return new JsonResult(favoriteOffers);
                        }
                    }
                    
                    // Hiç favori yoksa boþ liste döndür
                    System.Diagnostics.Debug.WriteLine("Fallback - No favorites found, returning empty list");
                    return new JsonResult(new List<object>());
                }
                catch (Exception innerEx)
                {
                    // Son çare - minimal liste
                    System.Diagnostics.Debug.WriteLine($"Final fallback error: {innerEx.Message}");
                    return new JsonResult(new List<object>());
                }
            }
        }
    }

    public class SyncFavoritesRequest
    {
        public List<int> FavoriteIds { get; set; } = new();
    }
}
