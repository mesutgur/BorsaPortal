using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class BlogCategoryController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public BlogCategoryController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/BlogCategory
        public ActionResult Index()
        {
            var categories = _context.BlogCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new BlogCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    PostCount = c.Posts.Count(p => !p.IsDeleted)
                })
                .ToList();

            return View(categories);
        }

        // GET: Admin/BlogCategory/Create
        public ActionResult Create()
        {
            return View(new BlogCategoryViewModel { DisplayOrder = 0 });
        }

        // POST: Admin/BlogCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BlogCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if slug already exists
                    if (_context.BlogCategories.Any(c => c.Slug == model.Slug && !c.IsDeleted))
                    {
                        ModelState.AddModelError("Slug", "Bu slug zaten kullanılıyor");
                        return View(model);
                    }

                    var category = new BlogCategory
                    {
                        Name = model.Name,
                        Slug = model.Slug.ToLowerInvariant(),
                        Description = model.Description,
                        DisplayOrder = model.DisplayOrder,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.BlogCategories.Add(category);
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} kategorisi başarıyla oluşturuldu.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Kategori oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/BlogCategory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _context.BlogCategories.Find(id);
            if (category == null || category.IsDeleted)
            {
                return HttpNotFound();
            }

            var model = new BlogCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder
            };

            return View(model);
        }

        // POST: Admin/BlogCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BlogCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = _context.BlogCategories.Find(model.Id);
                    if (category == null || category.IsDeleted)
                    {
                        return HttpNotFound();
                    }

                    // Check if slug already exists (excluding current category)
                    if (_context.BlogCategories.Any(c => c.Slug == model.Slug && c.Id != model.Id && !c.IsDeleted))
                    {
                        ModelState.AddModelError("Slug", "Bu slug zaten kullanılıyor");
                        return View(model);
                    }

                    category.Name = model.Name;
                    category.Slug = model.Slug.ToLowerInvariant();
                    category.Description = model.Description;
                    category.DisplayOrder = model.DisplayOrder;
                    category.UpdatedAt = DateTime.UtcNow;

                    // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                    _context.Entry(category).State = EntityState.Modified;
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} kategorisi başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Kategori güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/BlogCategory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = _context.BlogCategories
                .Include(c => c.Posts)
                .FirstOrDefault(c => c.Id == id);

            if (category == null || category.IsDeleted)
            {
                return HttpNotFound();
            }

            var model = new BlogCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                PostCount = category.Posts.Count(p => !p.IsDeleted)
            };

            return View(model);
        }

        // POST: Admin/BlogCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = _context.BlogCategories
                .Include(c => c.Posts)
                .FirstOrDefault(c => c.Id == id);

            if (category != null)
            {
                var activePostsCount = category.Posts.Count(p => !p.IsDeleted);
                if (activePostsCount > 0)
                {
                    SetErrorMessage($"Bu kategoride {activePostsCount} adet yazı bulunmaktadır. Önce yazıları silin veya başka kategoriye taşıyın.");
                    return RedirectToAction("Delete", new { id });
                }

                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                SetSuccessMessage($"{category.Name} kategorisi başarıyla silindi.");
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
