using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public SettingsController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Settings
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

            if (preferences == null)
            {
                // Create default preferences
                preferences = new UserPreference
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.UserPreferences.Add(preferences);
                await _context.SaveChangesAsync();
            }

            return View(preferences);
        }

        // POST: Settings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(UserPreference model)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var preferences = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

                if (preferences == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı tercihleri bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Update notification settings
                preferences.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
                preferences.SmsNotificationsEnabled = model.SmsNotificationsEnabled;
                preferences.PushNotificationsEnabled = model.PushNotificationsEnabled;
                preferences.NotifyOnKapDisclosures = model.NotifyOnKapDisclosures;
                preferences.NotifyOnPriceChanges = model.NotifyOnPriceChanges;
                preferences.NotifyOnNewsPublished = model.NotifyOnNewsPublished;

                // Update display settings
                preferences.PreferredLanguage = model.PreferredLanguage;
                preferences.PreferredCurrency = model.PreferredCurrency;
                preferences.ItemsPerPage = model.ItemsPerPage;

                // Update alert settings
                preferences.PriceChangeAlertPercentage = model.PriceChangeAlertPercentage;
                preferences.DailyDigestEnabled = model.DailyDigestEnabled;
                preferences.DailyDigestTime = model.DailyDigestTime;

                // Update privacy settings
                preferences.ProfileVisible = model.ProfileVisible;
                preferences.PortfolioVisible = model.PortfolioVisible;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ayarlarınız başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Settings/UserProfile
        public async Task<ActionResult> UserProfile()
        {
            var userId = User.Identity.GetUserId();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Settings/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile(string firstName, string lastName, string phoneNumber)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("UserProfile");
                }

                user.FirstName = firstName;
                user.LastName = lastName;
                user.PhoneNumber = phoneNumber;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profil bilgileriniz güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("UserProfile");
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
