using BorsaPortal.Domain.Entities;
using BorsaPortal.Domain.Enums;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class BlogPostController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public BlogPostController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/BlogPost
        public ActionResult Index(int? categoryId, PublishStatus? status)
        {
            var query = _context.BlogPosts
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted);

            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            var posts = query
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            ViewBag.CategoryId = categoryId;
            ViewBag.Status = status;
            ViewBag.Categories = GetCategoriesSelectList();
            ViewBag.Statuses = GetStatusesSelectList();

            return View(posts);
        }

        // GET: Admin/BlogPost/Create
        public ActionResult Create()
        {
            var viewModel = new BlogPostViewModel
            {
                Status = PublishStatus.Draft,
                Categories = GetCategoriesSelectList(),
                Statuses = GetStatusesSelectList()
            };

            return View(viewModel);
        }

        // POST: Admin/BlogPost/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(BlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var post = new BlogPost
                    {
                        Title = model.Title,
                        Slug = model.Slug,
                        Body = model.Body,
                        Summary = model.Summary,
                        FeaturedImageUrl = model.FeaturedImageUrl,
                        SeoTitle = model.SeoTitle,
                        SeoDescription = model.SeoDescription,
                        SeoKeywords = model.SeoKeywords,
                        Status = model.Status,
                        PublishedAt = model.Status == PublishStatus.Published ? DateTime.Now : model.PublishedAt,
                        CategoryId = model.CategoryId,
                        AuthorId = User.Identity.GetUserId(),
                        ViewCount = 0,
                        CommentCount = 0,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    _context.BlogPosts.Add(post);
                    _context.SaveChanges();

                    SetSuccessMessage("Blog yazısı başarıyla oluşturuldu.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Blog yazısı oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            model.Categories = GetCategoriesSelectList();
            model.Statuses = GetStatusesSelectList();
            return View(model);
        }

        // GET: Admin/BlogPost/Edit/5
        public ActionResult Edit(int id)
        {
            var post = _context.BlogPosts.FirstOrDefault(b => b.Id == id && !b.IsDeleted);

            if (post == null)
            {
                SetErrorMessage("Blog yazısı bulunamadı.");
                return RedirectToAction("Index");
            }

            var viewModel = new BlogPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Slug = post.Slug,
                Body = post.Body,
                Summary = post.Summary,
                FeaturedImageUrl = post.FeaturedImageUrl,
                SeoTitle = post.SeoTitle,
                SeoDescription = post.SeoDescription,
                SeoKeywords = post.SeoKeywords,
                Status = post.Status,
                PublishedAt = post.PublishedAt,
                CategoryId = post.CategoryId,
                ViewCount = post.ViewCount,
                CommentCount = post.CommentCount,
                Categories = GetCategoriesSelectList(),
                Statuses = GetStatusesSelectList()
            };

            return View(viewModel);
        }

        // POST: Admin/BlogPost/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(BlogPostViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var post = _context.BlogPosts.Find(model.Id);

                    if (post == null || post.IsDeleted)
                    {
                        SetErrorMessage("Blog yazısı bulunamadı.");
                        return RedirectToAction("Index");
                    }

                    post.Title = model.Title;
                    post.Slug = model.Slug;
                    post.Body = model.Body;
                    post.Summary = model.Summary;
                    post.FeaturedImageUrl = model.FeaturedImageUrl;
                    post.SeoTitle = model.SeoTitle;
                    post.SeoDescription = model.SeoDescription;
                    post.SeoKeywords = model.SeoKeywords;
                    post.Status = model.Status;
                    post.CategoryId = model.CategoryId;
                    post.UpdatedAt = DateTime.Now;

                    // Eğer yayına alınıyorsa ve henüz yayın tarihi yoksa, şimdi ekle
                    if (model.Status == PublishStatus.Published && !post.PublishedAt.HasValue)
                    {
                        post.PublishedAt = DateTime.Now;
                    }

                    // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                    _context.Entry(post).State = EntityState.Modified;
                    _context.SaveChanges();

                    SetSuccessMessage("Blog yazısı başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Blog yazısı güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            model.Categories = GetCategoriesSelectList();
            model.Statuses = GetStatusesSelectList();
            return View(model);
        }

        // GET: Admin/BlogPost/Delete/5
        public ActionResult Delete(int id)
        {
            var post = _context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefault(b => b.Id == id && !b.IsDeleted);

            if (post == null)
            {
                SetErrorMessage("Blog yazısı bulunamadı.");
                return RedirectToAction("Index");
            }

            return View(post);
        }

        // POST: Admin/BlogPost/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var post = _context.BlogPosts.Find(id);

            if (post != null)
            {
                post.IsDeleted = true;
                post.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                SetSuccessMessage("Blog yazısı başarıyla silindi.");
            }
            else
            {
                SetErrorMessage("Blog yazısı bulunamadı.");
            }

            return RedirectToAction("Index");
        }

        private SelectList GetCategoriesSelectList()
        {
            var categories = _context.BlogCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();

            return new SelectList(categories, "Id", "Name");
        }

        private SelectList GetStatusesSelectList()
        {
            var statuses = Enum.GetValues(typeof(PublishStatus))
                .Cast<PublishStatus>()
                .Select(s => new
                {
                    Value = (int)s,
                    Text = GetStatusDisplayName(s)
                })
                .ToList();

            return new SelectList(statuses, "Value", "Text");
        }

        private string GetStatusDisplayName(PublishStatus status)
        {
            switch (status)
            {
                case PublishStatus.Draft:
                    return "Taslak";
                case PublishStatus.Published:
                    return "Yayında";
                case PublishStatus.Archived:
                    return "Arşivlendi";
                default:
                    return status.ToString();
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
