using BorsaPortal.Domain.Enums;
using BorsaPortal.Infrastructure.Data;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BorsaPortal.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public BlogController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Blog
        public ActionResult Index(int? categoryId, int page = 1)
        {
            int pageSize = 12;

            var query = _context.BlogPosts
                .Where(b => !b.IsDeleted && b.Status == PublishStatus.Published);

            // Kategori filtresi
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
                var category = _context.BlogCategories.Find(categoryId.Value);
                ViewBag.SelectedCategory = category?.Name;
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var posts = query
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Kategorileri eager load
            foreach (var post in posts)
            {
                post.Category = _context.BlogCategories.Find(post.CategoryId);
                if (!string.IsNullOrEmpty(post.AuthorId))
                {
                    post.Author = _context.Users.Find(post.AuthorId);
                }
            }

            // Sayfalama bilgileri
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.CategoryId = categoryId;

            // Kategoriler
            ViewBag.Categories = _context.BlogCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();

            return View(posts);
        }

        // GET: Blog/Details/slug
        public ActionResult Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index");
            }

            var post = _context.BlogPosts
                .FirstOrDefault(b => b.Slug == slug && !b.IsDeleted && b.Status == PublishStatus.Published);

            if (post == null)
            {
                return HttpNotFound();
            }

            // Görüntülenme sayısını artır
            post.ViewCount++;
            _context.SaveChanges();

            // İlişkili verileri yükle
            post.Category = _context.BlogCategories.Find(post.CategoryId);
            if (!string.IsNullOrEmpty(post.AuthorId))
            {
                post.Author = _context.Users.Find(post.AuthorId);
            }

            // İlgili yazılar (aynı kategoriden)
            var relatedPosts = _context.BlogPosts
                .Where(b => !b.IsDeleted &&
                           b.Status == PublishStatus.Published &&
                           b.CategoryId == post.CategoryId &&
                           b.Id != post.Id)
                .OrderByDescending(b => b.PublishedAt ?? b.CreatedAt)
                .Take(4)
                .ToList();

            // İlgili yazıların kategorilerini yükle
            foreach (var relatedPost in relatedPosts)
            {
                relatedPost.Category = _context.BlogCategories.Find(relatedPost.CategoryId);
            }

            ViewBag.RelatedPosts = relatedPosts;

            return View(post);
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
