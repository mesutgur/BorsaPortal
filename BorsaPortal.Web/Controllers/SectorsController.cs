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
    public class SectorsController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public SectorsController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: Sectors
        public async Task<ActionResult> Index()
        {
            try
            {
                var sectors = await _context.Sectors
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                // Her sektördeki şirket sayısını hesapla
                var sectorStats = new List<SectorStatViewModel>();

                foreach (var sector in sectors)
                {
                    var companyCount = await _context.Companies
                        .Where(c => c.SectorId == sector.Id && !c.IsDeleted)
                        .CountAsync();

                    sectorStats.Add(new SectorStatViewModel
                    {
                        Sector = sector,
                        CompanyCount = companyCount
                    });
                }

                return View(sectorStats);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SectorsController.Index: {ex.Message}");
                TempData["ErrorMessage"] = "Sektörler yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Sectors/Analysis
        public async Task<ActionResult> Analysis()
        {
            try
            {
                var sectors = await _context.Sectors
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var sectorPerformance = new List<SectorPerformanceViewModel>();

                foreach (var sector in sectors)
                {
                    var companies = await _context.Companies
                        .Where(c => c.SectorId == sector.Id && !c.IsDeleted)
                        .Take(10) // Her sektörden ilk 10 şirketi al (performans için)
                        .ToListAsync();

                    if (!companies.Any())
                        continue;

                    // Şirket fiyatlarını çek
                    var symbols = companies.Select(c => c.Code).ToList();
                    var prices = await _stockPriceService.GetStockPricesAsync(symbols);

                    // Ortalama değişim hesapla
                    var successfulPrices = prices.Values.Where(p => p.IsSuccess && p.DayChangePercent.HasValue).ToList();

                    if (successfulPrices.Any())
                    {
                        var avgChange = successfulPrices.Average(p => p.DayChangePercent.Value);
                        var totalVolume = successfulPrices.Sum(p => p.Volume ?? 0);
                        var positiveCount = successfulPrices.Count(p => p.DayChangePercent > 0);
                        var negativeCount = successfulPrices.Count(p => p.DayChangePercent < 0);

                        sectorPerformance.Add(new SectorPerformanceViewModel
                        {
                            SectorId = sector.Id,
                            SectorName = sector.Name,
                            CompanyCount = companies.Count,
                            AverageChange = avgChange,
                            TotalVolume = totalVolume,
                            PositiveStockCount = positiveCount,
                            NegativeStockCount = negativeCount,
                            LastUpdate = DateTime.Now
                        });
                    }
                }

                // Performansa göre sırala
                sectorPerformance = sectorPerformance.OrderByDescending(s => s.AverageChange).ToList();

                return View(sectorPerformance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SectorsController.Analysis: {ex.Message}");
                TempData["ErrorMessage"] = "Sektörel analiz yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        // GET: Sectors/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var sector = await _context.Sectors
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (sector == null)
                {
                    return HttpNotFound();
                }

                var companies = await _context.Companies
                    .Where(c => c.SectorId == id && !c.IsDeleted)
                    .OrderBy(c => c.Code)
                    .ToListAsync();

                ViewBag.Companies = companies;

                return View(sector);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SectorsController.Details: {ex.Message}");
                TempData["ErrorMessage"] = "Sektör detayları yüklenirken bir hata oluştu.";
                return RedirectToAction("Index");
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

    // ViewModels
    public class SectorStatViewModel
    {
        public Sector Sector { get; set; }
        public int CompanyCount { get; set; }
    }

    public class SectorPerformanceViewModel
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; }
        public int CompanyCount { get; set; }
        public decimal AverageChange { get; set; }
        public long TotalVolume { get; set; }
        public int PositiveStockCount { get; set; }
        public int NegativeStockCount { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
