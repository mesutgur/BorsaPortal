using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Domain.Enums;

namespace BorsaPortal.Web.Controllers
{
    public class KapNotificationsController : Controller
    {
        private readonly BorsaPortalDbContext _context;

        public KapNotificationsController()
        {
            _context = new BorsaPortalDbContext();
        }

        // GET: KapNotifications
        public ActionResult Index(int? companyId, KapNotificationType? notificationType, int page = 1)
        {
            int pageSize = 20;

            var query = _context.KapNotifications.Where(k => !k.IsDeleted);

            // Şirkete göre filtre
            if (companyId.HasValue)
            {
                query = query.Where(k => k.CompanyId == companyId.Value);
                var company = _context.Companies.Find(companyId.Value);
                ViewBag.SelectedCompany = company?.Name;
                ViewBag.SelectedCompanyCode = company?.Code;
            }

            // Bildirim türüne göre filtre
            if (notificationType.HasValue)
            {
                query = query.Where(k => k.NotificationType == notificationType.Value);
                ViewBag.SelectedNotificationType = GetNotificationTypeDisplayName(notificationType.Value);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var notifications = query
                .OrderByDescending(k => k.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Şirket bilgilerini ekle (eager loading)
            foreach (var notification in notifications)
            {
                notification.Company = _context.Companies.Find(notification.CompanyId);
            }

            // Sayfalama bilgileri
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.CompanyId = companyId;
            ViewBag.NotificationType = notificationType;

            // Filtreler için seçenekler
            ViewBag.Companies = _context.Companies.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            // Tüm bildirim türleri
            var notificationTypes = Enum.GetValues(typeof(KapNotificationType))
                .Cast<KapNotificationType>()
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = GetNotificationTypeDisplayName(t)
                })
                .ToList();
            ViewBag.NotificationTypes = notificationTypes;

            return View(notifications);
        }

        private string GetNotificationTypeDisplayName(KapNotificationType type)
        {
            switch (type)
            {
                case KapNotificationType.FinancialStatement:
                    return "Mali Tablo";
                case KapNotificationType.GeneralAssembly:
                    return "Genel Kurul";
                case KapNotificationType.OwnershipStructure:
                    return "Ortaklık Yapısı";
                case KapNotificationType.MaterialDisclosure:
                    return "Özel Durum Açıklaması";
                case KapNotificationType.Dividend:
                    return "Kar Payı";
                case KapNotificationType.BoardDecision:
                    return "Yönetim Kurulu Kararı";
                case KapNotificationType.CapitalIncrease:
                    return "Sermaye Artırımı";
                case KapNotificationType.MergerAcquisition:
                    return "Birleşme/Devralma";
                case KapNotificationType.Related:
                    return "İlişkili Taraf İşlemleri";
                case KapNotificationType.Other:
                    return "Diğer";
                default:
                    return type.ToString();
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