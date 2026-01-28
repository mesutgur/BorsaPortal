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
    public class MarketIndexController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public MarketIndexController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/MarketIndex
        public ActionResult Index()
        {
            var indices = _context.MarketIndices
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.Code)
                .ToList();

            return View(indices);
        }

        // GET: Admin/MarketIndex/Create
        public ActionResult Create()
        {
            return View(new MarketIndexViewModel());
        }

        // POST: Admin/MarketIndex/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MarketIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var marketIndex = new MarketIndex
                    {
                        Code = model.Code,
                        Name = model.Name,
                        Description = model.Description,
                        CurrentValue = model.CurrentValue,
                        AveragePE = model.AveragePE,
                        AveragePB = model.AveragePB,
                        DailyChange = model.DailyChange,
                        TotalMarketCap = model.TotalMarketCap,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.MarketIndices.Add(marketIndex);
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} endeksi başarıyla oluşturuldu.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Endeks oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/MarketIndex/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var marketIndex = _context.MarketIndices.Find(id);
            if (marketIndex == null || marketIndex.IsDeleted)
            {
                return HttpNotFound();
            }

            var model = new MarketIndexViewModel
            {
                Id = marketIndex.Id,
                Code = marketIndex.Code,
                Name = marketIndex.Name,
                Description = marketIndex.Description,
                CurrentValue = marketIndex.CurrentValue,
                AveragePE = marketIndex.AveragePE,
                AveragePB = marketIndex.AveragePB,
                DailyChange = marketIndex.DailyChange,
                TotalMarketCap = marketIndex.TotalMarketCap
            };

            return View(model);
        }

        // POST: Admin/MarketIndex/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MarketIndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var marketIndex = _context.MarketIndices.Find(model.Id);
                    if (marketIndex == null || marketIndex.IsDeleted)
                    {
                        return HttpNotFound();
                    }

                    marketIndex.Code = model.Code;
                    marketIndex.Name = model.Name;
                    marketIndex.Description = model.Description;
                    marketIndex.CurrentValue = model.CurrentValue;
                    marketIndex.AveragePE = model.AveragePE;
                    marketIndex.AveragePB = model.AveragePB;
                    marketIndex.DailyChange = model.DailyChange;
                    marketIndex.TotalMarketCap = model.TotalMarketCap;
                    marketIndex.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(marketIndex).State = EntityState.Modified;
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} endeksi başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Endeks güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/MarketIndex/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var marketIndex = _context.MarketIndices.Find(id);
            if (marketIndex == null || marketIndex.IsDeleted)
            {
                return HttpNotFound();
            }

            return View(marketIndex);
        }

        // POST: Admin/MarketIndex/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var marketIndex = _context.MarketIndices.Find(id);
            if (marketIndex != null)
            {
                marketIndex.IsDeleted = true;
                marketIndex.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                SetSuccessMessage($"{marketIndex.Name} endeksi başarıyla silindi.");
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
