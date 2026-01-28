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

namespace BorsaPortal.Web.Controllers
{
    public class ScreenerController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public ScreenerController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: Screener
        public async Task<ActionResult> Index(ScreenerFilter filter)
        {
            try
            {
                // Load sectors for dropdown
                var sectors = await _context.Sectors
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                ViewBag.Sectors = sectors;

                // Get all companies
                var query = _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted);

                // Apply sector filter
                if (filter.SectorId.HasValue)
                {
                    query = query.Where(c => c.SectorId == filter.SectorId.Value);
                }

                var companies = await query.Take(50).ToListAsync(); // Limit to 50 companies for performance

                // Get price data for companies
                var symbols = companies.Select(c => c.Code).ToList();
                var priceData = await _stockPriceService.GetStockPricesAsync(symbols);

                // Create screener results
                var results = new List<ScreenerResult>();

                foreach (var company in companies)
                {
                    if (priceData.TryGetValue(company.Code, out var price) && price.IsSuccess)
                    {
                        var result = new ScreenerResult
                        {
                            Company = company,
                            CurrentPrice = price.CurrentPrice,
                            Change = price.DayChange ?? 0,
                            ChangePercent = price.DayChangePercent ?? 0,
                            Volume = price.Volume ?? 0,
                            DayHigh = price.DayHigh ?? 0,
                            DayLow = price.DayLow ?? 0
                        };

                        // Apply filters
                        if (!PassesFilter(result, filter))
                            continue;

                        results.Add(result);
                    }
                }

                // Apply sorting
                results = ApplySorting(results, filter.SortBy ?? "changePercent");

                ViewBag.Filter = filter;
                return View(results);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ScreenerController.Index: {ex.Message}");
                TempData["ErrorMessage"] = "Tarayıcı yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        private bool PassesFilter(ScreenerResult result, ScreenerFilter filter)
        {
            // Price range
            if (filter.MinPrice.HasValue && result.CurrentPrice < filter.MinPrice.Value)
                return false;

            if (filter.MaxPrice.HasValue && result.CurrentPrice > filter.MaxPrice.Value)
                return false;

            // Volume
            if (filter.MinVolume.HasValue && result.Volume < filter.MinVolume.Value)
                return false;

            // Change percent
            if (filter.MinChangePercent.HasValue && result.ChangePercent < filter.MinChangePercent.Value)
                return false;

            if (filter.MaxChangePercent.HasValue && result.ChangePercent > filter.MaxChangePercent.Value)
                return false;

            // Gainers only
            if (filter.GainersOnly && result.ChangePercent <= 0)
                return false;

            // Losers only
            if (filter.LosersOnly && result.ChangePercent >= 0)
                return false;

            return true;
        }

        private List<ScreenerResult> ApplySorting(List<ScreenerResult> results, string sortBy)
        {
            switch (sortBy?.ToLower())
            {
                case "price":
                    return results.OrderByDescending(r => r.CurrentPrice).ToList();
                case "volume":
                    return results.OrderByDescending(r => r.Volume).ToList();
                case "changepercent":
                    return results.OrderByDescending(r => r.ChangePercent).ToList();
                case "code":
                    return results.OrderBy(r => r.Company.Code).ToList();
                case "name":
                    return results.OrderBy(r => r.Company.Name).ToList();
                default:
                    return results.OrderByDescending(r => r.ChangePercent).ToList();
            }
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

    // Models
    public class ScreenerFilter
    {
        public int? SectorId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public long? MinVolume { get; set; }
        public decimal? MinChangePercent { get; set; }
        public decimal? MaxChangePercent { get; set; }
        public bool GainersOnly { get; set; }
        public bool LosersOnly { get; set; }
        public string SortBy { get; set; }
    }

    public class ScreenerResult
    {
        public Company Company { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public long Volume { get; set; }
        public decimal DayHigh { get; set; }
        public decimal DayLow { get; set; }
    }
}
