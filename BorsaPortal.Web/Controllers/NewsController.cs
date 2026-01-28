using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorsaPortal.Infrastructure.Data;

namespace BorsaPortal.Web.Controllers
{
    public class NewsController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public NewsController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: News
        public ActionResult Index(int? sectorId, int? companyId, int page = 1)
        {
            int pageSize = 12;

            var query = _context.News.Where(n => !n.IsDeleted);

            // Sektöre göre filtre
            if (sectorId.HasValue)
            {
                query = query.Where(n => n.SectorId == sectorId.Value);
                ViewBag.SelectedSector = _context.Sectors.Find(sectorId.Value)?.Name;
            }

            // Şirkete göre filtre
            if (companyId.HasValue)
            {
                query = query.Where(n => n.CompanyId == companyId.Value);
                ViewBag.SelectedCompany = _context.Companies.Find(companyId.Value)?.Name;
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var news = query
                .OrderByDescending(n => n.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Sayfalama bilgileri
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SectorId = sectorId;
            ViewBag.CompanyId = companyId;

            // Filtreler için seçenekler
            ViewBag.Sectors = _context.Sectors.Where(s => !s.IsDeleted).OrderBy(s => s.Name).ToList();
            ViewBag.Companies = _context.Companies.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            return View(news);
        }

        // GET: News/Details/5
        public ActionResult Details(int id)
        {
            var news = _context.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted);

            if (news == null)
            {
                return HttpNotFound();
            }

            // Görüntülenme sayısını artır
            news.ViewCount++;
            _context.SaveChanges();

            // İlgili haberler (aynı sektör veya şirket)
            var relatedNews = _context.News
                .Where(n => !n.IsDeleted && n.Id != id)
                .Where(n => (news.SectorId.HasValue && n.SectorId == news.SectorId) ||
                           (news.CompanyId.HasValue && n.CompanyId == news.CompanyId))
                .OrderByDescending(n => n.PublishDate)
                .Take(4)
                .ToList();

            ViewBag.RelatedNews = relatedNews;

            // Şirket ve sektör bilgisi
            if (news.CompanyId.HasValue)
            {
                ViewBag.Company = _context.Companies.Find(news.CompanyId.Value);
            }

            if (news.SectorId.HasValue)
            {
                ViewBag.Sector = _context.Sectors.Find(news.SectorId.Value);
            }

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
