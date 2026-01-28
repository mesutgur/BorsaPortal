using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public FavoritesController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Favorites
        public async Task<ActionResult> Index(string type = null)
        {
            var userId = User.Identity.GetUserId();

            var query = _context.UserFavorites
                .Where(f => f.UserId == userId && !f.IsDeleted);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(f => f.EntityType == type);
            }

            var favorites = await query
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            // Get actual entities for favorites - optimized to avoid N+1 queries
            var favoriteViewModels = new System.Collections.Generic.List<FavoriteViewModel>();

            // Group favorites by entity type to batch load
            var newsFavorites = favorites.Where(f => f.EntityType == "News").ToList();
            var kapFavorites = favorites.Where(f => f.EntityType == "KapNotification").ToList();
            var blogFavorites = favorites.Where(f => f.EntityType == "BlogPost").ToList();

            // Batch load all news items
            if (newsFavorites.Any())
            {
                var newsIds = newsFavorites.Select(f => f.EntityId).ToList();
                var newsItems = await _context.News
                    .Where(n => newsIds.Contains(n.Id) && !n.IsDeleted)
                    .ToDictionaryAsync(n => n.Id);

                foreach (var fav in newsFavorites)
                {
                    if (newsItems.TryGetValue(fav.EntityId, out var news))
                    {
                        favoriteViewModels.Add(new FavoriteViewModel
                        {
                            Id = fav.Id,
                            EntityType = fav.EntityType,
                            EntityId = fav.EntityId,
                            AddedAt = fav.AddedAt,
                            Notes = fav.Notes,
                            Title = news.Title,
                            Url = Url.Action("Details", "News", new { id = news.Id }),
                            PublishedDate = news.PublishDate
                        });
                    }
                }
            }

            // Batch load all KAP notifications
            if (kapFavorites.Any())
            {
                var kapIds = kapFavorites.Select(f => f.EntityId).ToList();
                var kapItems = await _context.KapNotifications
                    .Where(k => kapIds.Contains(k.Id) && !k.IsDeleted)
                    .ToDictionaryAsync(k => k.Id);

                foreach (var fav in kapFavorites)
                {
                    if (kapItems.TryGetValue(fav.EntityId, out var kap))
                    {
                        favoriteViewModels.Add(new FavoriteViewModel
                        {
                            Id = fav.Id,
                            EntityType = fav.EntityType,
                            EntityId = fav.EntityId,
                            AddedAt = fav.AddedAt,
                            Notes = fav.Notes,
                            Title = kap.Title,
                            Url = Url.Action("Details", "Kap", new { id = kap.Id }),
                            PublishedDate = kap.PublishDate
                        });
                    }
                }
            }

            // Batch load all blog posts
            if (blogFavorites.Any())
            {
                var blogIds = blogFavorites.Select(f => f.EntityId).ToList();
                var blogItems = await _context.BlogPosts
                    .Where(b => blogIds.Contains(b.Id) && !b.IsDeleted)
                    .ToDictionaryAsync(b => b.Id);

                foreach (var fav in blogFavorites)
                {
                    if (blogItems.TryGetValue(fav.EntityId, out var blog))
                    {
                        favoriteViewModels.Add(new FavoriteViewModel
                        {
                            Id = fav.Id,
                            EntityType = fav.EntityType,
                            EntityId = fav.EntityId,
                            AddedAt = fav.AddedAt,
                            Notes = fav.Notes,
                            Title = blog.Title,
                            Url = Url.Action("Details", "Blog", new { slug = blog.Slug }),
                            PublishedDate = blog.PublishedAt
                        });
                    }
                }
            }

            // Re-sort by AddedAt to maintain order
            favoriteViewModels = favoriteViewModels.OrderByDescending(f => f.AddedAt).ToList();

            ViewBag.FilterType = type;
            return View(favoriteViewModels);
        }

        // POST: Favorites/Add
        [HttpPost]
        public async Task<JsonResult> Add(string entityType, int entityId, string notes = null)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                // Check if already in favorites
                var exists = await _context.UserFavorites
                    .AnyAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted);

                if (exists)
                {
                    return Json(new { success = false, message = "Bu içerik zaten favorilerinizde." });
                }

                var favorite = new UserFavorite
                {
                    UserId = userId,
                    EntityType = entityType,
                    EntityId = entityId,
                    Notes = notes,
                    AddedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.UserFavorites.Add(favorite);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Favorilerinize eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: Favorites/Remove
        [HttpPost]
        public async Task<JsonResult> Remove(string entityType, int entityId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var favorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted);

                if (favorite == null)
                {
                    return Json(new { success = false, message = "Favori bulunamadı." });
                }

                favorite.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Favorilerden çıkarıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: Favorites/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var favorite = await _context.UserFavorites
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && !f.IsDeleted);

                if (favorite == null)
                {
                    TempData["ErrorMessage"] = "Favori bulunamadı.";
                    return RedirectToAction("Index");
                }

                favorite.IsDeleted = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Favori silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Check if item is in favorites
        [HttpGet]
        public async Task<JsonResult> IsFavorite(string entityType, int entityId)
        {
            var userId = User.Identity.GetUserId();
            var exists = await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted);

            return Json(new { isFavorite = exists }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModel for displaying favorites
    public class FavoriteViewModel
    {
        public int Id { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? PublishedDate { get; set; }
        public string Notes { get; set; }
    }
}
