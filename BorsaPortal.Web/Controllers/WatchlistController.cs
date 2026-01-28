using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class WatchlistController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public WatchlistController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: Watchlist
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();

            var watchlist = await _context.Watchlists
                .Include(w => w.Company)
                .Include(w => w.Company.Sector)
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            // Fetch real-time prices from API
            var stockPrices = new Dictionary<string, StockPriceData>();
            if (watchlist.Any())
            {
                var symbols = watchlist.Select(w => w.Company.Code).Distinct().ToList();
                stockPrices = await _stockPriceService.GetStockPricesAsync(symbols);
            }

            var companies = await _context.Companies
                .Include(c => c.Sector)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Sector.Name)
                .ThenBy(c => c.Code)
                .Select(c => new WatchlistCompanyDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    SectorName = c.Sector.Name
                })
                .ToListAsync();

            ViewBag.Companies = companies;
            ViewBag.StockPrices = stockPrices;

            return View(watchlist);
        }

        // POST: Watchlist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(int companyId, string notes, decimal? targetPrice, bool notifyOnTargetPrice = false)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                // Check if already in watchlist
                var exists = await _context.Watchlists
                    .AnyAsync(w => w.UserId == userId && w.CompanyId == companyId && !w.IsDeleted);

                if (exists)
                {
                    TempData["ErrorMessage"] = "Bu şirket zaten izleme listenizde.";
                    return RedirectToAction("Index");
                }

                var watchlistItem = new Watchlist
                {
                    UserId = userId,
                    CompanyId = companyId,
                    Notes = notes,
                    TargetPrice = targetPrice,
                    NotifyOnTargetPrice = notifyOnTargetPrice,
                    AddedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Watchlists.Add(watchlistItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şirket izleme listenize eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Watchlist/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(int id, string notes, decimal? targetPrice, bool notifyOnTargetPrice = false)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var item = await _context.Watchlists
                    .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId && !w.IsDeleted);

                if (item == null)
                {
                    TempData["ErrorMessage"] = "İzleme listesi kalemi bulunamadı.";
                    return RedirectToAction("Index");
                }

                item.Notes = notes;
                item.TargetPrice = targetPrice;
                item.NotifyOnTargetPrice = notifyOnTargetPrice;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İzleme listesi güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Watchlist/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var item = await _context.Watchlists
                    .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId && !w.IsDeleted);

                if (item == null)
                {
                    TempData["ErrorMessage"] = "İzleme listesi kalemi bulunamadı.";
                    return RedirectToAction("Index");
                }

                item.IsDeleted = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Şirket izleme listenizden çıkarıldı.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // AJAX: Add to watchlist from company page
        [HttpPost]
        public async Task<JsonResult> AddToWatchlistAjax(int companyId)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                // Check if already in watchlist
                var exists = await _context.Watchlists
                    .AnyAsync(w => w.UserId == userId && w.CompanyId == companyId && !w.IsDeleted);

                if (exists)
                {
                    return Json(new { success = false, message = "Bu şirket zaten izleme listenizde." });
                }

                var watchlistItem = new Watchlist
                {
                    UserId = userId,
                    CompanyId = companyId,
                    AddedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Watchlists.Add(watchlistItem);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şirket izleme listenize eklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // AJAX: Remove from watchlist
        [HttpPost]
        public async Task<JsonResult> RemoveFromWatchlistAjax(int companyId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var item = await _context.Watchlists
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.CompanyId == companyId && !w.IsDeleted);

                if (item == null)
                {
                    return Json(new { success = false, message = "İzleme listesi kalemi bulunamadı." });
                }

                item.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Şirket izleme listenizden çıkarıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // AJAX: Check if company is in watchlist
        [HttpGet]
        public async Task<JsonResult> IsInWatchlist(int companyId)
        {
            var userId = User.Identity.GetUserId();
            var exists = await _context.Watchlists
                .AnyAsync(w => w.UserId == userId && w.CompanyId == companyId && !w.IsDeleted);

            return Json(new { isInWatchlist = exists }, JsonRequestBehavior.AllowGet);
        }

        // POST: Watchlist/UpdatePrices
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePrices()
        {
            try
            {
                var userId = User.Identity.GetUserId();

                var watchlist = await _context.Watchlists
                    .Include(w => w.Company)
                    .Where(w => w.UserId == userId && !w.IsDeleted)
                    .ToListAsync();

                if (!watchlist.Any())
                {
                    TempData["WarningMessage"] = "İzleme listenizde şirket bulunmuyor.";
                    return RedirectToAction("Index");
                }

                var symbols = watchlist.Select(w => w.Company.Code).Distinct().ToList();
                var prices = await _stockPriceService.GetStockPricesAsync(symbols);

                int successCount = prices.Count(p => p.Value.IsSuccess);
                int failCount = prices.Count - successCount;

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"{successCount} şirketin fiyatı güncellendi.";
                }
                else
                {
                    TempData["WarningMessage"] = "Fiyatlar güncellenemedi. Lütfen daha sonra tekrar deneyin.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
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

    // DTO for company dropdown in watchlist
    public class WatchlistCompanyDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SectorName { get; set; }
    }
}
