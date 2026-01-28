using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NewsAutomationController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public NewsAutomationController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/NewsAutomation
        public ActionResult Index()
        {
            var rssSources = _context.RssFeedSources
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.IsActive)
                .ThenByDescending(r => r.LastFetchedAt)
                .ToList();

            var recentNews = _context.News
                .Where(n => !n.IsDeleted && n.IsAutoFetched)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToList();

            ViewBag.RecentNews = recentNews;
            ViewBag.TotalActiveSources = rssSources.Count(r => r.IsActive);
            ViewBag.TotalSources = rssSources.Count;
            ViewBag.TotalAutoFetchedNews = _context.News.Count(n => !n.IsDeleted && n.IsAutoFetched);

            return View(rssSources);
        }

        // POST: Admin/NewsAutomation/SyncAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SyncAll()
        {
            try
            {
                var rssFeedService = new RssFeedService();
                var newsSyncService = new NewsSyncService(_context, rssFeedService);

                var result = await newsSyncService.SyncAllFeedsAsync();

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Beklenmeyen bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/NewsAutomation/SyncFeed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SyncFeed(int id)
        {
            try
            {
                var rssFeedService = new RssFeedService();
                var newsSyncService = new NewsSyncService(_context, rssFeedService);

                var result = await newsSyncService.SyncFeedByIdAsync(id);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Beklenmeyen bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/NewsAutomation/AutoFetchedNews
        public ActionResult AutoFetchedNews(int page = 1)
        {
            int pageSize = 20;
            var skip = (page - 1) * pageSize;

            var newsQuery = _context.News
                .Include(n => n.Company)
                .Include(n => n.Sector)
                .Where(n => !n.IsDeleted && n.IsAutoFetched);

            var totalCount = newsQuery.Count();
            var news = newsQuery
                .OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (totalCount + pageSize - 1) / pageSize;
            ViewBag.TotalCount = totalCount;

            return View(news);
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
