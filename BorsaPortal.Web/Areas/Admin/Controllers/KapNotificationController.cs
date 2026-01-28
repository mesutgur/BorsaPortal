using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Domain.Enums;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class KapNotificationController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;

        public KapNotificationController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: Admin/KapNotification
        public ActionResult Index(int? companyId)
        {
            var query = _context.KapNotifications
                .Include(k => k.Company)
                .Where(k => !k.IsDeleted);

            if (companyId.HasValue)
            {
                query = query.Where(k => k.CompanyId == companyId.Value);
            }

            var notifications = query
                .OrderByDescending(k => k.PublishDate)
                .Take(100)
                .ToList();

            ViewBag.Companies = new SelectList(_context.Companies.Where(c => !c.IsDeleted).OrderBy(c => c.Name), "Id", "Name", companyId);

            return View(notifications);
        }

        // POST: Admin/KapNotification/SyncFromKap
        [HttpPost]
        public async Task<ActionResult> SyncFromKap(int days = 1)
        {
            try
            {
                var kapApiService = new KapApiService();
                var syncService = new KapSyncService(_context, kapApiService);

                var result = await syncService.SyncRecentNotificationsAsync(days);

                if (result.Success)
                {
                    SetSuccessMessage(result.Message);
                }
                else
                {
                    SetErrorMessage(result.Message);
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"KAP senkronizasyon hatası: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/KapNotification/Create
        public ActionResult Create()
        {
            var model = new KapNotificationViewModel
            {
                PublishDate = DateTime.Now,
                Companies = GetCompanySelectList(),
                NotificationTypes = GetNotificationTypeSelectList()
            };

            return View(model);
        }

        // POST: Admin/KapNotification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KapNotificationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var notification = new KapNotification
                    {
                        CompanyId = model.CompanyId,
                        Title = model.Title,
                        Summary = model.Summary,
                        Content = model.Content,
                        PublishDate = model.PublishDate,
                        NotificationType = model.NotificationType,
                        NotificationNumber = model.NotificationNumber,
                        SourceUrl = model.SourceUrl,
                        IsAutoFetched = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.KapNotifications.Add(notification);
                    _context.SaveChanges();

                    SetSuccessMessage("KAP bildirimi başarıyla eklendi.");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    SetErrorMessage($"KAP bildirimi eklenirken bir hata oluştu: {ex.Message}");
                }
            }

            model.Companies = GetCompanySelectList();
            model.NotificationTypes = GetNotificationTypeSelectList();
            return View(model);
        }

        // GET: Admin/KapNotification/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var notification = _context.KapNotifications
                .Include(k => k.Company)
                .FirstOrDefault(k => k.Id == id);

            if (notification == null || notification.IsDeleted)
            {
                return HttpNotFound();
            }

            return View(notification);
        }

        // POST: Admin/KapNotification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var notification = _context.KapNotifications.Find(id);
            if (notification != null)
            {
                notification.IsDeleted = true;
                notification.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                SetSuccessMessage("KAP bildirimi başarıyla silindi.");
            }

            return RedirectToAction("Index");
        }

        private SelectList GetCompanySelectList(int? selectedValue = null)
        {
            var companies = _context.Companies
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();

            return new SelectList(companies, "Id", "Name", selectedValue);
        }

        private SelectList GetNotificationTypeSelectList(KapNotificationType? selectedValue = null)
        {
            var types = Enum.GetValues(typeof(KapNotificationType))
                .Cast<KapNotificationType>()
                .Select(e => new
                {
                    Value = (int)e,
                    Text = GetNotificationTypeDisplayName(e)
                })
                .ToList();

            return new SelectList(types, "Value", "Text", (int?)selectedValue);
        }

        private string GetNotificationTypeDisplayName(KapNotificationType type)
        {
            switch (type)
            {
                case KapNotificationType.FinancialStatement: return "Mali Tablo";
                case KapNotificationType.GeneralAssembly: return "Genel Kurul";
                case KapNotificationType.OwnershipStructure: return "Ortaklık Yapısı";
                case KapNotificationType.MaterialDisclosure: return "Özel Durum Açıklaması";
                case KapNotificationType.Dividend: return "Kar Payı";
                case KapNotificationType.BoardDecision: return "Yönetim Kurulu Kararı";
                case KapNotificationType.CapitalIncrease: return "Sermaye Artırımı";
                case KapNotificationType.MergerAcquisition: return "Birleşme/Devralma";
                case KapNotificationType.Related: return "İlişkili Taraf İşlemleri";
                case KapNotificationType.Other: return "Diğer";
                default: return type.ToString();
            }
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
