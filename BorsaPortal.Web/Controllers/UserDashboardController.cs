using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using System.Collections.Generic;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public UserDashboardController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: UserDashboard
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var portfolioService = new PortfolioService(_context);

            // Portfolio Summary
            var portfolioSummary = await portfolioService.GetPortfolioSummaryAsync(userId);
            ViewBag.PortfolioSummary = portfolioSummary;

            // Watchlist with real-time prices
            var watchlist = await _context.Watchlists
                .Include(w => w.Company)
                .Include(w => w.Company.Sector)
                .Where(w => w.UserId == userId && !w.IsDeleted)
                .OrderByDescending(w => w.AddedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.Watchlist = watchlist;

            // Fetch real-time prices for watchlist
            var stockPrices = new Dictionary<string, StockPriceData>();
            if (watchlist.Any())
            {
                var symbols = watchlist.Select(w => w.Company.Code).Distinct().ToList();
                stockPrices = await _stockPriceService.GetStockPricesAsync(symbols);
            }
            ViewBag.StockPrices = stockPrices;

            // Latest KAP Notifications
            var latestKapNotifications = await _context.KapNotifications
                .Include(k => k.Company)
                .Where(k => !k.IsDeleted)
                .OrderByDescending(k => k.PublishDate)
                .Take(5)
                .ToListAsync();
            ViewBag.LatestKapNotifications = latestKapNotifications;

            // Latest News
            var latestNews = await _context.News
                .Where(n => !n.IsDeleted)
                .OrderByDescending(n => n.PublishDate)
                .Take(5)
                .ToListAsync();
            ViewBag.LatestNews = latestNews;

            // Recent Favorites
            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId && !f.IsDeleted)
                .OrderByDescending(f => f.AddedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.Favorites = favorites;

            // User Preferences
            var preferences = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            ViewBag.Preferences = preferences;

            return View();
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
