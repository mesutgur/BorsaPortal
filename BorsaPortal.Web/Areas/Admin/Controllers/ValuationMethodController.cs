using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class ValuationMethodController : AdminBaseController
    {
        private readonly IValuationService _valuationService;

        public ValuationMethodController()
        {
            var context = new BorsaPortalDbContext();
            _valuationService = new ValuationService(context);
        }

        // GET: Admin/ValuationMethod
        public async Task<ActionResult> Index()
        {
            var methods = await _valuationService.GetAllValuationMethodsAsync();
            return View(methods);
        }

        // GET: Admin/ValuationMethod/Create
        public ActionResult Create()
        {
            return View(new ValuationMethodViewModel());
        }

        // POST: Admin/ValuationMethod/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ValuationMethodViewModel model)
        {
            if (ModelState.IsValid)
            {
                var method = new ValuationMethod
                {
                    Name = model.Name,
                    Description = model.Description,
                    MethodType = model.MethodType,
                    Formula = model.Formula,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder
                };

                await _valuationService.CreateValuationMethodAsync(method);
                SetSuccessMessage($"{model.Name} değerleme yöntemi başarıyla oluşturuldu.");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/ValuationMethod/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var method = await _valuationService.GetValuationMethodByIdAsync(id.Value);
            if (method == null)
            {
                return HttpNotFound();
            }

            var model = new ValuationMethodViewModel
            {
                Id = method.Id,
                Name = method.Name,
                Description = method.Description,
                MethodType = method.MethodType,
                Formula = method.Formula,
                IsActive = method.IsActive,
                DisplayOrder = method.DisplayOrder
            };

            return View(model);
        }

        // POST: Admin/ValuationMethod/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ValuationMethodViewModel model)
        {
            if (ModelState.IsValid)
            {
                var method = await _valuationService.GetValuationMethodByIdAsync(model.Id);
                if (method == null)
                {
                    return HttpNotFound();
                }

                method.Name = model.Name;
                method.Description = model.Description;
                method.MethodType = model.MethodType;
                method.Formula = model.Formula;
                method.IsActive = model.IsActive;
                method.DisplayOrder = model.DisplayOrder;

                await _valuationService.UpdateValuationMethodAsync(method);
                SetSuccessMessage($"{model.Name} değerleme yöntemi başarıyla güncellendi.");
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Admin/ValuationMethod/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var method = await _valuationService.GetValuationMethodByIdAsync(id);
            if (method == null)
            {
                return Json(new { success = false, message = "Değerleme yöntemi bulunamadı." });
            }

            await _valuationService.DeleteValuationMethodAsync(id);
            return Json(new { success = true, message = $"{method.Name} değerleme yöntemi başarıyla silindi." });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                (_valuationService as IDisposable)?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
