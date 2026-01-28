using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;

namespace BorsaPortal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly MarketDataService _marketDataService;
        private readonly IStockPriceService _stockPriceService;

        public HomeController()
        {
            _context = new BorsaPortalDbContext();
            _marketDataService = new MarketDataService();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                // Piyasa özeti verilerini al
                var marketSummary = await _marketDataService.GetMarketSummaryAsync();
                ViewBag.MarketSummary = marketSummary;

                // Öne çıkan haberler (en son 5 öne çıkan)
                var featuredNews = _context.News
                    .Where(n => !n.IsDeleted && n.IsFeatured)
                    .OrderByDescending(n => n.PublishDate)
                    .Take(5)
                    .ToList();

                // Son haberler (en son 6 haber)
                var latestNews = _context.News
                    .Where(n => !n.IsDeleted)
                    .OrderByDescending(n => n.PublishDate)
                    .Take(6)
                    .ToList();

                // Son KAP bildirimleri (en son 10 bildirim)
                var latestKapNotifications = _context.KapNotifications
                    .Where(k => !k.IsDeleted)
                    .OrderByDescending(k => k.PublishDate)
                    .Take(10)
                    .ToList();

                ViewBag.FeaturedNews = featuredNews;
                ViewBag.LatestNews = latestNews;
                ViewBag.LatestKapNotifications = latestKapNotifications;

                return View();
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging framework)
                System.Diagnostics.Debug.WriteLine($"Error in HomeController.Index: {ex.Message}");

                // Provide empty collections to prevent view errors
                ViewBag.FeaturedNews = new List<BorsaPortal.Domain.Entities.News>();
                ViewBag.LatestNews = new List<BorsaPortal.Domain.Entities.News>();
                ViewBag.LatestKapNotifications = new List<BorsaPortal.Domain.Entities.KapNotification>();

                TempData["ErrorMessage"] = "Ana sayfa yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return View();
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Borsa Portal - Türkiye'nin en güncel borsa ve finans haber platformu.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "İletişim bilgilerimiz.";

            return View();
        }

        // API: Kazandıran Hisseler (Top Gainers)
        [HttpGet]
        public async Task<JsonResult> GetTopGainers(int count = 5)
        {
            try
            {
                var companies = await _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => Guid.NewGuid()) // Random selection
                    .Take(20)
                    .ToListAsync();

                var symbols = companies.Select(c => c.Code).ToList();
                var priceData = await _stockPriceService.GetStockPricesAsync(symbols);

                var gainers = new List<StockCardData>();

                foreach (var company in companies)
                {
                    if (priceData.TryGetValue(company.Code, out var price) &&
                        price.IsSuccess &&
                        price.DayChangePercent.HasValue &&
                        price.DayChangePercent.Value > 0)
                    {
                        gainers.Add(new StockCardData
                        {
                            Code = company.Code,
                            Name = company.Name,
                            SectorName = company.Sector?.Name ?? "Diğer",
                            Price = price.CurrentPrice,
                            Change = price.DayChange ?? 0,
                            ChangePercent = price.DayChangePercent ?? 0,
                            Volume = price.Volume ?? 0
                        });
                    }
                }

                var topGainers = gainers
                    .OrderByDescending(g => g.ChangePercent)
                    .Take(count)
                    .ToList();

                return Json(new { success = true, data = topGainers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTopGainers: {ex.Message}");
                return Json(new { success = false, message = "Veri alınırken hata oluştu." }, JsonRequestBehavior.AllowGet);
            }
        }

        // API: Kaybettiren Hisseler (Top Losers)
        [HttpGet]
        public async Task<JsonResult> GetTopLosers(int count = 5)
        {
            try
            {
                var companies = await _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => Guid.NewGuid())
                    .Take(20)
                    .ToListAsync();

                var symbols = companies.Select(c => c.Code).ToList();
                var priceData = await _stockPriceService.GetStockPricesAsync(symbols);

                var losers = new List<StockCardData>();

                foreach (var company in companies)
                {
                    if (priceData.TryGetValue(company.Code, out var price) &&
                        price.IsSuccess &&
                        price.DayChangePercent.HasValue &&
                        price.DayChangePercent.Value < 0)
                    {
                        losers.Add(new StockCardData
                        {
                            Code = company.Code,
                            Name = company.Name,
                            SectorName = company.Sector?.Name ?? "Diğer",
                            Price = price.CurrentPrice,
                            Change = price.DayChange ?? 0,
                            ChangePercent = price.DayChangePercent ?? 0,
                            Volume = price.Volume ?? 0
                        });
                    }
                }

                var topLosers = losers
                    .OrderBy(l => l.ChangePercent)
                    .Take(count)
                    .ToList();

                return Json(new { success = true, data = topLosers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetTopLosers: {ex.Message}");
                return Json(new { success = false, message = "Veri alınırken hata oluştu." }, JsonRequestBehavior.AllowGet);
            }
        }

        // API: Yüksek Hacimli Hisseler (High Volume Stocks)
        [HttpGet]
        public async Task<JsonResult> GetHighVolumeStocks(int count = 5)
        {
            try
            {
                var companies = await _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => Guid.NewGuid())
                    .Take(20)
                    .ToListAsync();

                var symbols = companies.Select(c => c.Code).ToList();
                var priceData = await _stockPriceService.GetStockPricesAsync(symbols);

                var highVolumeStocks = new List<StockCardData>();

                foreach (var company in companies)
                {
                    if (priceData.TryGetValue(company.Code, out var price) &&
                        price.IsSuccess &&
                        price.Volume.HasValue &&
                        price.Volume.Value > 0)
                    {
                        highVolumeStocks.Add(new StockCardData
                        {
                            Code = company.Code,
                            Name = company.Name,
                            SectorName = company.Sector?.Name ?? "Diğer",
                            Price = price.CurrentPrice,
                            Change = price.DayChange ?? 0,
                            ChangePercent = price.DayChangePercent ?? 0,
                            Volume = price.Volume ?? 0
                        });
                    }
                }

                var topHighVolume = highVolumeStocks
                    .OrderByDescending(h => h.Volume)
                    .Take(count)
                    .ToList();

                return Json(new { success = true, data = topHighVolume }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetHighVolumeStocks: {ex.Message}");
                return Json(new { success = false, message = "Veri alınırken hata oluştu." }, JsonRequestBehavior.AllowGet);
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

    // Model for stock card data
    public class StockCardData
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SectorName { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public long Volume { get; set; }
    }
}