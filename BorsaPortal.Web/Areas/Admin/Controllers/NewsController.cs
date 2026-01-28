using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class NewsController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public NewsController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/News
        public ActionResult Index(int? companyId, int? sectorId)
        {
            var query = _context.News
                .Include(n => n.Company)
                .Include(n => n.Sector)
                .Where(n => !n.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(n => n.CompanyId == companyId.Value);
            }

            if (sectorId.HasValue)
            {
                query = query.Where(n => n.SectorId == sectorId.Value);
            }

            var news = query
                .OrderByDescending(n => n.PublishDate)
                .Take(100)
                .ToList();

            ViewBag.CompanyId = companyId;
            ViewBag.SectorId = sectorId;
            ViewBag.Companies = GetCompaniesSelectList();
            ViewBag.Sectors = GetSectorsSelectList();

            return View(news);
        }

        // GET: Admin/News/Create
        public ActionResult Create()
        {
            var viewModel = new NewsViewModel
            {
                PublishDate = DateTime.Now,
                Companies = GetCompaniesSelectList(),
                Sectors = GetSectorsSelectList()
            };

            return View(viewModel);
        }

        // POST: Admin/News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var news = new News
                    {
                        Title = model.Title,
                        Content = model.Content,
                        Summary = model.Summary,
                        PublishDate = model.PublishDate,
                        Source = model.Source,
                        SourceUrl = model.SourceUrl,
                        ImageUrl = model.ImageUrl,
                        IsFeatured = model.IsFeatured,
                        CompanyId = model.CompanyId,
                        SectorId = model.SectorId,
                        ViewCount = 0,
                        IsAutoFetched = false,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    _context.News.Add(news);
                    _context.SaveChanges();

                    SetSuccessMessage("Haber başarıyla eklendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Haber eklenirken bir hata oluştu: {ex.Message}");
                }
            }

            model.Companies = GetCompaniesSelectList();
            model.Sectors = GetSectorsSelectList();
            return View(model);
        }

        // GET: Admin/News/Edit/5
        public ActionResult Edit(int id)
        {
            var news = _context.News.FirstOrDefault(n => n.Id == id && !n.IsDeleted);

            if (news == null)
            {
                SetErrorMessage("Haber bulunamadı.");
                return RedirectToAction("Index");
            }

            var viewModel = new NewsViewModel
            {
                Id = news.Id,
                Title = news.Title,
                Content = news.Content,
                Summary = news.Summary,
                PublishDate = news.PublishDate,
                Source = news.Source,
                SourceUrl = news.SourceUrl,
                ImageUrl = news.ImageUrl,
                IsFeatured = news.IsFeatured,
                CompanyId = news.CompanyId,
                SectorId = news.SectorId,
                ViewCount = news.ViewCount,
                Companies = GetCompaniesSelectList(),
                Sectors = GetSectorsSelectList()
            };

            return View(viewModel);
        }

        // POST: Admin/News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var news = _context.News.Find(model.Id);

                    if (news == null || news.IsDeleted)
                    {
                        SetErrorMessage("Haber bulunamadı.");
                        return RedirectToAction("Index");
                    }

                    news.Title = model.Title;
                    news.Content = model.Content;
                    news.Summary = model.Summary;
                    news.PublishDate = model.PublishDate;
                    news.Source = model.Source;
                    news.SourceUrl = model.SourceUrl;
                    news.ImageUrl = model.ImageUrl;
                    news.IsFeatured = model.IsFeatured;
                    news.CompanyId = model.CompanyId;
                    news.SectorId = model.SectorId;
                    news.UpdatedAt = DateTime.Now;

                    // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                    _context.Entry(news).State = EntityState.Modified;
                    _context.SaveChanges();

                    SetSuccessMessage("Haber başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Haber güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            model.Companies = GetCompaniesSelectList();
            model.Sectors = GetSectorsSelectList();
            return View(model);
        }

        // GET: Admin/News/Delete/5
        public ActionResult Delete(int id)
        {
            var news = _context.News
                .Include(n => n.Company)
                .Include(n => n.Sector)
                .FirstOrDefault(n => n.Id == id && !n.IsDeleted);

            if (news == null)
            {
                SetErrorMessage("Haber bulunamadı.");
                return RedirectToAction("Index");
            }

            return View(news);
        }

        // POST: Admin/News/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var news = _context.News.Find(id);

            if (news != null)
            {
                news.IsDeleted = true;
                news.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                SetSuccessMessage("Haber başarıyla silindi.");
            }
            else
            {
                SetErrorMessage("Haber bulunamadı.");
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/News/DeleteAllAutoFetched
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAllAutoFetched()
        {
            try
            {
                var autoFetchedNews = _context.News.Where(n => n.IsAutoFetched && !n.IsDeleted).ToList();
                var count = autoFetchedNews.Count;

                foreach (var news in autoFetchedNews)
                {
                    news.IsDeleted = true;
                    news.UpdatedAt = DateTime.Now;
                }

                _context.SaveChanges();
                SetSuccessMessage($"{count} adet otomatik çekilen haber başarıyla silindi.");
            }
            catch (Exception ex)
            {
                SetErrorMessage($"Haber silme işlemi sırasında hata oluştu: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // POST: Admin/News/FetchContentFromUrl
        [HttpPost]
        public async Task<JsonResult> FetchContentFromUrl(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return Json(new { success = false, message = "URL boş olamaz." });
                }

                var contentFetcher = new ContentFetcherService();
                var fetchedContent = await contentFetcher.FetchContentFromUrlAsync(url);

                return Json(new
                {
                    success = true,
                    title = fetchedContent.Title,
                    summary = fetchedContent.Summary,
                    content = fetchedContent.Content,
                    imageUrl = fetchedContent.ImageUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"İçerik çekilirken hata oluştu: {ex.Message}"
                });
            }
        }

        private SelectList GetCompaniesSelectList()
        {
            var companies = _context.Companies
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Code)
                .Select(c => new
                {
                    c.Id,
                    DisplayText = c.Code + " - " + c.Name
                })
                .ToList();

            return new SelectList(companies, "Id", "DisplayText");
        }

        private SelectList GetSectorsSelectList()
        {
            var sectors = _context.Sectors
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToList();

            return new SelectList(sectors, "Id", "Name");
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
