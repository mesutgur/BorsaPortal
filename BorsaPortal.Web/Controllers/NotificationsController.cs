using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            var stockPriceService = new StockPriceService(_context, portfolioService);
            _notificationService = new NotificationService(_context, stockPriceService);
        }

        // GET: Notifications
        public async Task<ActionResult> Index(string filter = "all")
        {
            var userId = User.Identity.GetUserId();

            var query = _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsDeleted);

            // Apply filter
            switch (filter.ToLower())
            {
                case "unread":
                    query = query.Where(n => !n.IsRead);
                    break;
                case "targetprice":
                    query = query.Where(n => n.NotificationType == "TargetPrice");
                    break;
                case "kap":
                    query = query.Where(n => n.NotificationType == "KAP");
                    break;
                case "system":
                    query = query.Where(n => n.NotificationType == "System");
                    break;
            }

            var notifications = await query
                .OrderByDescending(n => n.NotificationDate)
                .Take(50)
                .ToListAsync();

            ViewBag.Filter = filter;
            ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return View(notifications);
        }

        // POST: Notifications/MarkAsRead/5
        [HttpPost]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var userId = User.Identity.GetUserId();
            await _notificationService.MarkAsReadAsync(id, userId);

            return Json(new { success = true });
        }

        // POST: Notifications/MarkAllAsRead
        [HttpPost]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = User.Identity.GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);

            TempData["SuccessMessage"] = "Tüm bildirimler okundu olarak işaretlendi.";
            return RedirectToAction("Index");
        }

        // POST: Notifications/Delete/5
        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var notification = await _context.UserNotifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);

                if (notification != null)
                {
                    notification.IsDeleted = true;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Bildirim silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bildirim bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Notifications/GetUnreadCount (AJAX)
        [HttpGet]
        public async Task<JsonResult> GetUnreadCount()
        {
            var userId = User.Identity.GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        // GET: Notifications/Preferences
        public async Task<ActionResult> Preferences()
        {
            var userId = User.Identity.GetUserId();

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

            if (preferences == null)
            {
                preferences = new Domain.Entities.UserPreference
                {
                    UserId = userId,
                    NotifyOnPriceChanges = true,
                    NotifyOnKapDisclosures = true,
                    NotifyOnNewsPublished = true,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };
                _context.UserPreferences.Add(preferences);
                await _context.SaveChangesAsync();
            }

            return View(preferences);
        }

        // POST: Notifications/UpdatePreferences
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePreferences(bool targetPriceNotifications, bool kapNotifications, bool newsNotifications, bool emailNotifications, string dailyDigestTime)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var preferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

                if (preferences == null)
                {
                    preferences = new Domain.Entities.UserPreference
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };
                    _context.UserPreferences.Add(preferences);
                }

                preferences.NotifyOnPriceChanges = targetPriceNotifications;
                preferences.NotifyOnKapDisclosures = kapNotifications;
                preferences.NotifyOnNewsPublished = newsNotifications;
                preferences.EmailNotificationsEnabled = emailNotifications;
                preferences.DailyDigestTime = dailyDigestTime;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bildirim tercihleri güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Preferences");
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
}
