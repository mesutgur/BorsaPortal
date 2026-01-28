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
    public class CompaniesController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public CompaniesController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: Companies
        public async Task<ActionResult> Index(string search, int? sectorId, string sortBy = "code", int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted);

                // Arama filtresi
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToUpper();
                    query = query.Where(c => c.Code.Contains(search) || c.Name.ToUpper().Contains(search));
                }

                // Sektör filtresi
                if (sectorId.HasValue)
                {
                    query = query.Where(c => c.SectorId == sectorId.Value);
                }

                // Sıralama
                switch (sortBy?.ToLower())
                {
                    case "name":
                        query = query.OrderBy(c => c.Name);
                        break;
                    case "sector":
                        query = query.OrderBy(c => c.Sector.Name).ThenBy(c => c.Code);
                        break;
                    default:
                        query = query.OrderBy(c => c.Code);
                        break;
                }

                // Toplam sayı
                var totalCount = await query.CountAsync();
                ViewBag.TotalCount = totalCount;
                ViewBag.PageSize = pageSize;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Sayfalama
                var companies = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Filtreleme için sektör listesi
                var sectors = await _context.Sectors
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                ViewBag.Sectors = sectors;
                ViewBag.Search = search;
                ViewBag.SectorId = sectorId;
                ViewBag.SortBy = sortBy;

                return View(companies);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CompaniesController.Index: {ex.Message}");
                TempData["ErrorMessage"] = "Şirketler listesi yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Companies/Details/5
        public async Task<ActionResult> Details(int? id, string code)
        {
            try
            {
                var userId = User.Identity.GetUserId();

                Domain.Entities.Company company;

                // Code ile arama (öncelikli)
                if (!string.IsNullOrWhiteSpace(code))
                {
                    company = await _context.Companies
                        .Include(c => c.Sector)
                        .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && !c.IsDeleted);
                }
                // Id ile arama
                else if (id.HasValue)
                {
                    company = await _context.Companies
                        .Include(c => c.Sector)
                        .FirstOrDefaultAsync(c => c.Id == id.Value && !c.IsDeleted);
                }
                else
                {
                    return HttpNotFound();
                }

                if (company == null)
                {
                    return HttpNotFound();
                }

                // Get real-time stock price
                var priceData = await _stockPriceService.GetStockPriceAsync(company.Code);
                ViewBag.PriceData = priceData;

                // Check if in watchlist
                var isInWatchlist = await _context.Watchlists
                    .AnyAsync(w => w.UserId == userId && w.CompanyId == company.Id && !w.IsDeleted);
                ViewBag.IsInWatchlist = isInWatchlist;

                // Check if in portfolio
                var portfolioItem = await _context.PortfolioItems
                    .Include(p => p.Portfolio)
                    .FirstOrDefaultAsync(p => p.Portfolio.UserId == userId && p.CompanyId == company.Id && !p.IsDeleted);
                ViewBag.PortfolioItem = portfolioItem;

                // Get recent news
                var recentNews = await _context.News
                    .Where(n => n.CompanyId == company.Id && !n.IsDeleted)
                    .OrderByDescending(n => n.PublishDate)
                    .Take(5)
                    .ToListAsync();
                ViewBag.RecentNews = recentNews;

                // Get recent KAP notifications
                var recentKap = await _context.KapNotifications
                    .Where(k => k.CompanyId == company.Id && !k.IsDeleted)
                    .OrderByDescending(k => k.PublishDate)
                    .Take(5)
                    .ToListAsync();
                ViewBag.RecentKap = recentKap;

                // Get historical price data for chart (last 30 days)
                var historicalData = await _stockPriceService.GetHistoricalPricesAsync(company.Code, 30);
                ViewBag.HistoricalData = historicalData;

                return View(company);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CompaniesController.Details: {ex.Message}");
                TempData["ErrorMessage"] = "Şirket detayları yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Companies/Search
        [HttpGet]
        public async Task<JsonResult> Search(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return Json(new object[] { }, JsonRequestBehavior.AllowGet);
                }

                query = query.Trim().ToUpper();

                var companies = await _context.Companies
                    .Include(c => c.Sector)
                    .Where(c => !c.IsDeleted &&
                                (c.Code.Contains(query) || c.Name.ToUpper().Contains(query)))
                    .OrderBy(c => c.Code)
                    .Take(10)
                    .Select(c => new
                    {
                        Code = c.Code,
                        Name = c.Name,
                        SectorName = c.Sector.Name ?? ""
                    })
                    .ToListAsync();

                return Json(companies, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CompaniesController.Search: {ex.Message}");
                return Json(new object[] { }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Companies/GetHistoricalData
        [HttpGet]
        public async Task<JsonResult> GetHistoricalData(string symbol, int days = 30)
        {
            try
            {
                var historicalData = await _stockPriceService.GetHistoricalPricesAsync(symbol, days);
                return Json(historicalData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetHistoricalData: {ex.Message}");
                return Json(new { error = "Geçmiş veriler yüklenirken hata oluştu." }, JsonRequestBehavior.AllowGet);
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
}
