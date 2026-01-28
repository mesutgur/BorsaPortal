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
    public class SectorController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public SectorController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/Sector
        public ActionResult Index()
        {
            var sectors = _context.Sectors
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToList();

            return View(sectors);
        }

        // GET: Admin/Sector/Create
        public ActionResult Create()
        {
            return View(new SectorViewModel());
        }

        // POST: Admin/Sector/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SectorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var sector = new Sector
                    {
                        Name = model.Name,
                        Code = model.Code,
                        Description = model.Description,
                        AveragePE = model.AveragePE,
                        AveragePB = model.AveragePB,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Sectors.Add(sector);
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} sektörü başarıyla oluşturuldu.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Sektör oluşturulurken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/Sector/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sector = _context.Sectors.Find(id);
            if (sector == null || sector.IsDeleted)
            {
                return HttpNotFound();
            }

            var model = new SectorViewModel
            {
                Id = sector.Id,
                Name = sector.Name,
                Code = sector.Code,
                Description = sector.Description,
                AveragePE = sector.AveragePE,
                AveragePB = sector.AveragePB
            };

            return View(model);
        }

        // POST: Admin/Sector/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SectorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var sector = _context.Sectors.Find(model.Id);
                    if (sector == null || sector.IsDeleted)
                    {
                        return HttpNotFound();
                    }

                    sector.Name = model.Name;
                    sector.Code = model.Code;
                    sector.Description = model.Description;
                    sector.AveragePE = model.AveragePE;
                    sector.AveragePB = model.AveragePB;
                    sector.UpdatedAt = DateTime.UtcNow;

                    // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                    _context.Entry(sector).State = EntityState.Modified;
                    _context.SaveChanges();

                    SetSuccessMessage($"{model.Name} sektörü başarıyla güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"Sektör güncellenirken bir hata oluştu: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/Sector/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sector = _context.Sectors.Find(id);
            if (sector == null || sector.IsDeleted)
            {
                return HttpNotFound();
            }

            return View(sector);
        }

        // POST: Admin/Sector/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var sector = _context.Sectors.Find(id);
            if (sector != null)
            {
                sector.IsDeleted = true;
                sector.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                SetSuccessMessage($"{sector.Name} sektörü başarıyla silindi.");
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
