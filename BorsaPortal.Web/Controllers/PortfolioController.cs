using System;
using System.Collections.Generic;
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
    public class PortfolioController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly PortfolioService _portfolioService;
        private readonly IStockPriceService _stockPriceService;

        public PortfolioController()
        {
            _context = new BorsaPortalDbContext();
            _portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, _portfolioService);
        }

        // GET: Portfolio
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var summary = await _portfolioService.GetPortfolioSummaryAsync(userId);

            // Get companies with sector information
            var companies = await _context.Companies
                .Include(c => c.Sector)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Sector.Name)
                .ThenBy(c => c.Code)
                .Select(c => new CompanyDropdownDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    SectorName = c.Sector.Name
                })
                .ToListAsync();

            // Get user's watchlist
            var watchlistCompanyIds = await _context.Watchlists
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .Select(w => w.CompanyId)
                .ToListAsync();

            ViewBag.Companies = companies;
            ViewBag.WatchlistCompanyIds = watchlistCompanyIds;

            return View(summary);
        }

        // POST: Portfolio/Buy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Buy(int companyId, decimal? quantity, decimal? price)
        {
            try
            {
                // Validate inputs
                if (!quantity.HasValue || quantity.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen geçerli bir adet giriniz.";
                    return RedirectToAction("Index");
                }

                if (!price.HasValue || price.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen geçerli bir fiyat giriniz.";
                    return RedirectToAction("Index");
                }

                var userId = User.Identity.GetUserId();
                await _portfolioService.AddStockAsync(userId, companyId, quantity.Value, price.Value);

                TempData["SuccessMessage"] = "Hisse başarıyla portföyünüze eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Portfolio/Sell
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Sell(int companyId, decimal? quantity, decimal? price)
        {
            try
            {
                // Validate inputs
                if (!quantity.HasValue || quantity.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen geçerli bir adet giriniz.";
                    return RedirectToAction("Index");
                }

                if (!price.HasValue || price.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen geçerli bir fiyat giriniz.";
                    return RedirectToAction("Index");
                }

                var userId = User.Identity.GetUserId();
                var result = await _portfolioService.RemoveStockAsync(userId, companyId, quantity.Value, price.Value);

                if (result)
                {
                    TempData["SuccessMessage"] = "Satış işlemi başarıyla gerçekleştirildi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Portföyünüzde yeterli hisse bulunmuyor.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Portfolio/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var result = await _portfolioService.DeletePortfolioItemAsync(userId, id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Portföy kalemi silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Portföy kalemi bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Portfolio/UpdatePrices
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePrices()
        {
            try
            {
                var userId = User.Identity.GetUserId();

                // Get portfolio to check what stocks we have
                var portfolio = await _context.Portfolios
                    .Include(p => p.Items.Select(i => i.Company))
                    .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

                if (portfolio == null || !portfolio.Items.Any(i => !i.IsDeleted))
                {
                    TempData["WarningMessage"] = "Portföyünüzde hisse bulunmuyor.";
                    return RedirectToAction("Index");
                }

                var stockSymbols = string.Join(", ", portfolio.Items.Where(i => !i.IsDeleted).Select(i => i.Company.Code));
                System.Diagnostics.Debug.WriteLine($"UpdatePrices Controller: Updating prices for: {stockSymbols}");

                // Update prices from external API (Twelve Data)
                var success = await _stockPriceService.UpdatePortfolioPricesAsync(userId);

                System.Diagnostics.Debug.WriteLine($"UpdatePrices Controller: Update result = {success}");

                if (success)
                {
                    TempData["SuccessMessage"] = $"Hisse fiyatları başarıyla güncellendi: {stockSymbols}";
                }
                else
                {
                    TempData["WarningMessage"] = $"Fiyatlar güncellenemedi. Kontrol edilen hisseler: {stockSymbols}. Twelve Data API'sinden veri alınamadı. Lütfen hisse kodlarının doğru olduğundan veya API bağlantısının çalıştığından emin olun.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePrices Controller Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Portfolio/Analytics
        public async Task<ActionResult> Analytics()
        {
            var userId = User.Identity.GetUserId();
            var summary = await _portfolioService.GetPortfolioSummaryAsync(userId);

            return View(summary);
        }

        // POST: Portfolio/UpdatePrice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePrice(int portfolioItemId, decimal? newPrice)
        {
            try
            {
                if (!newPrice.HasValue || newPrice.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Lütfen geçerli bir fiyat giriniz.";
                    return RedirectToAction("Index");
                }

                var userId = User.Identity.GetUserId();
                var portfolio = await _portfolioService.GetOrCreatePortfolioAsync(userId);
                var item = portfolio.Items.FirstOrDefault(i => i.Id == portfolioItemId && !i.IsDeleted);

                if (item == null)
                {
                    TempData["ErrorMessage"] = "Portföy kalemi bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Update current price
                item.CurrentPrice = newPrice.Value;
                item.CurrentValue = item.Quantity * newPrice.Value;
                item.ProfitLoss = item.CurrentValue - item.TotalCost;
                item.ProfitLossPercentage = item.TotalCost > 0 ? (item.ProfitLoss / item.TotalCost) * 100 : 0;
                item.LastPriceUpdate = DateTime.Now;

                await _context.SaveChangesAsync();
                await _portfolioService.UpdatePortfolioValuesAsync(userId);

                TempData["SuccessMessage"] = $"{item.Company.Code} fiyatı güncellendi: ₺{newPrice.Value:N2}";
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

    // DTO for company dropdown
    public class CompanyDropdownDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string SectorName { get; set; }
    }
}
