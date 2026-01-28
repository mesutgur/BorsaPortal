using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Web.Areas.Admin.ViewModels;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RssFeedSourceController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public RssFeedSourceController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/RssFeedSource
        public ActionResult Index()
        {
            var sources = _context.RssFeedSources
                .Include(r => r.Sector)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(sources);
        }

        // GET: Admin/RssFeedSource/Create
        public ActionResult Create()
        {
            ViewBag.Sectors = GetSectorsSelectList();

            var model = new RssFeedSourceViewModel
            {
                IsActive = true,
                FetchIntervalMinutes = 60,
                AutoPublish = false
            };

            return View(model);
        }

        // POST: Admin/RssFeedSource/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RssFeedSourceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sectors = GetSectorsSelectList(model.SectorId);
                return View(model);
            }

            try
            {
                var source = new RssFeedSource
                {
                    Name = model.Name,
                    Description = model.Description,
                    FeedUrl = model.FeedUrl,
                    IsActive = model.IsActive,
                    FetchIntervalMinutes = model.FetchIntervalMinutes,
                    SectorId = model.SectorId,
                    IncludeKeywords = model.IncludeKeywords,
                    ExcludeKeywords = model.ExcludeKeywords,
                    AutoPublish = model.AutoPublish,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.RssFeedSources.Add(source);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "RSS feed kaynağı başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"RSS feed kaynağı oluşturulurken bir hata oluştu: {ex.Message}";
                ViewBag.Sectors = GetSectorsSelectList(model.SectorId);
                return View(model);
            }
        }

        // GET: Admin/RssFeedSource/Edit/5
        public ActionResult Edit(int id)
        {
            var source = _context.RssFeedSources
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);

            if (source == null)
                return HttpNotFound();

            var model = new RssFeedSourceViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                FeedUrl = source.FeedUrl,
                IsActive = source.IsActive,
                LastFetchedAt = source.LastFetchedAt,
                FetchIntervalMinutes = source.FetchIntervalMinutes,
                SectorId = source.SectorId,
                ItemsFetchedCount = source.ItemsFetchedCount,
                IncludeKeywords = source.IncludeKeywords,
                ExcludeKeywords = source.ExcludeKeywords,
                AutoPublish = source.AutoPublish
            };

            ViewBag.Sectors = GetSectorsSelectList(model.SectorId);

            return View(model);
        }

        // POST: Admin/RssFeedSource/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RssFeedSourceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sectors = GetSectorsSelectList(model.SectorId);
                return View(model);
            }

            try
            {
                var source = _context.RssFeedSources
                    .FirstOrDefault(r => r.Id == model.Id && !r.IsDeleted);

                if (source == null)
                    return HttpNotFound();

                source.Name = model.Name;
                source.Description = model.Description;
                source.FeedUrl = model.FeedUrl;
                source.IsActive = model.IsActive;
                source.FetchIntervalMinutes = model.FetchIntervalMinutes;
                source.SectorId = model.SectorId;
                source.IncludeKeywords = model.IncludeKeywords;
                source.ExcludeKeywords = model.ExcludeKeywords;
                source.AutoPublish = model.AutoPublish;

                // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                _context.Entry(source).State = EntityState.Modified;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "RSS feed kaynağı başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"RSS feed kaynağı güncellenirken bir hata oluştu: {ex.Message}";
                ViewBag.Sectors = GetSectorsSelectList(model.SectorId);
                return View(model);
            }
        }

        // GET: Admin/RssFeedSource/Delete/5
        public ActionResult Delete(int id)
        {
            var source = _context.RssFeedSources
                .Include(r => r.Sector)
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);

            if (source == null)
                return HttpNotFound();

            return View(source);
        }

        // POST: Admin/RssFeedSource/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var source = _context.RssFeedSources
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);

            if (source == null)
                return HttpNotFound();

            // Soft delete
            source.IsDeleted = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "RSS feed kaynağı başarıyla silindi.";
            return RedirectToAction("Index");
        }

        // POST: Admin/RssFeedSource/ToggleActive/5
        [HttpPost]
        public JsonResult ToggleActive(int id)
        {
            var source = _context.RssFeedSources
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);

            if (source == null)
                return Json(new { success = false, message = "Kaynak bulunamadı" });

            source.IsActive = !source.IsActive;
            _context.SaveChanges();

            return Json(new { success = true, isActive = source.IsActive });
        }

        private SelectList GetSectorsSelectList(int? selectedValue = null)
        {
            var sectors = _context.Sectors
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name })
                .ToList();

            return new SelectList(sectors, "Id", "Name", selectedValue);
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
