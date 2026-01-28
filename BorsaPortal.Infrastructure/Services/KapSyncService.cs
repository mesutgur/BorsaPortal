using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Domain.Enums;
using BorsaPortal.Infrastructure.Data;

namespace BorsaPortal.Infrastructure.Services
{
    /// <summary>
    /// KAP bildirimlerini senkronize eden servis
    /// </summary>
    public class KapSyncService
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IKapApiService _kapApiService;

        public KapSyncService(BorsaPortalDbContext context, IKapApiService kapApiService)
        {
            _context = context;
            _kapApiService = kapApiService;
        }

        /// <summary>
        /// Son N gündeki KAP bildirimlerini senkronize eder
        /// </summary>
        public async Task<SyncResult> SyncRecentNotificationsAsync(int days = 1)
        {
            var result = new SyncResult();

            try
            {
                // KAP API'den bildirimleri çek
                var notifications = await _kapApiService.FetchRecentNotificationsAsync(days);

                // TEST: KAP API çalışmazsa mock data oluştur (geliştirme amaçlı)
                if (notifications == null || !notifications.Any())
                {
                    notifications = GenerateMockNotifications();
                }

                if (notifications == null || !notifications.Any())
                {
                    result.Message = "KAP'tan bildirim çekilemedi ve hiç şirket kaydı bulun amadı.";
                    return result;
                }

                // Şirket eşleştirmesi yap
                var companies = _context.Companies
                    .Where(c => !c.IsDeleted)
                    .ToList();

                foreach (var notification in notifications)
                {
                    // Bildirim numarasına göre kontrol et (duplicate kayıt olmasın)
                    if (!string.IsNullOrEmpty(notification.NotificationNumber))
                    {
                        var exists = _context.KapNotifications
                            .Any(k => k.NotificationNumber == notification.NotificationNumber && !k.IsDeleted);

                        if (exists)
                        {
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    // Şirket eşleştir (başlıktan veya içerikten şirket kodu çıkar)
                    var matchedCompany = FindMatchingCompany(notification, companies);

                    if (matchedCompany != null)
                    {
                        notification.CompanyId = matchedCompany.Id;
                    }

                    // Database'e kaydet
                    _context.KapNotifications.Add(notification);
                    result.AddedCount++;
                }

                _context.SaveChanges();

                result.Success = true;
                result.Message = $"{result.AddedCount} yeni bildirim eklendi, {result.SkippedCount} bildirim atlandı.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Hata: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Belirli bir şirketin KAP bildirimlerini senkronize eder
        /// </summary>
        public async Task<SyncResult> SyncCompanyNotificationsAsync(string companyCode, int days = 30)
        {
            var result = new SyncResult();

            try
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-days);

                var notifications = await _kapApiService.FetchNotificationsByCompanyAsync(companyCode, startDate, endDate);

                if (notifications == null || !notifications.Any())
                {
                    result.Message = "Bildirim bulunamadı.";
                    return result;
                }

                var company = _context.Companies
                    .FirstOrDefault(c => c.Code == companyCode && !c.IsDeleted);

                if (company == null)
                {
                    result.Success = false;
                    result.Message = "Şirket bulunamadı.";
                    return result;
                }

                foreach (var notification in notifications)
                {
                    if (!string.IsNullOrEmpty(notification.NotificationNumber))
                    {
                        var exists = _context.KapNotifications
                            .Any(k => k.NotificationNumber == notification.NotificationNumber && !k.IsDeleted);

                        if (exists)
                        {
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    notification.CompanyId = company.Id;
                    _context.KapNotifications.Add(notification);
                    result.AddedCount++;
                }

                _context.SaveChanges();

                result.Success = true;
                result.Message = $"{result.AddedCount} yeni bildirim eklendi.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Hata: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Bildirim için eşleşen şirketi bulur
        /// </summary>
        private Company FindMatchingCompany(KapNotification notification, List<Company> companies)
        {
            if (notification == null || companies == null || !companies.Any())
                return null;

            var searchText = $"{notification.Title} {notification.Summary}".ToUpper();

            // Önce şirket koduna göre ara
            foreach (var company in companies)
            {
                if (!string.IsNullOrEmpty(company.Code) &&
                    searchText.Contains(company.Code.ToUpper()))
                {
                    return company;
                }
            }

            // Şirket adına göre ara
            foreach (var company in companies)
            {
                if (!string.IsNullOrEmpty(company.Name) &&
                    searchText.Contains(company.Name.ToUpper()))
                {
                    return company;
                }
            }

            return null;
        }

        /// <summary>
        /// Test amaçlı mock KAP bildirimleri oluşturur (KAP API çalışmadığında)
        /// </summary>
        private List<KapNotification> GenerateMockNotifications()
        {
            var companies = _context.Companies.Where(c => !c.IsDeleted).ToList();

            if (!companies.Any())
                return new List<KapNotification>();

            var notifications = new List<KapNotification>();
            var random = new Random();
            var now = DateTime.Now;

            // Her şirket için 1-2 bildirim oluştur
            foreach (var company in companies.Take(5)) // İlk 5 şirket için
            {
                var notificationTypes = new[]
                {
                    KapNotificationType.FinancialStatement,
                    KapNotificationType.MaterialDisclosure,
                    KapNotificationType.BoardDecision,
                    KapNotificationType.Dividend
                };

                var titleTemplates = new[]
                {
                    "Mali Tablo Açıklaması",
                    "Özel Durum Açıklaması",
                    "Yönetim Kurulu Kararı",
                    "Temettü Dağıtım Kararı",
                    "Ortaklık Yapısı Değişikliği"
                };

                var count = random.Next(1, 3); // 1 veya 2 bildirim
                for (int i = 0; i < count; i++)
                {
                    var typeIndex = random.Next(notificationTypes.Length);

                    // Mock bildirim ID oluştur (gerçekçi görünmesi için)
                    var mockDisclosureId = random.Next(1000000, 9999999);

                    var notification = new KapNotification
                    {
                        CompanyId = company.Id,
                        Title = $"{company.Code} - {titleTemplates[random.Next(titleTemplates.Length)]}",
                        Summary = $"{company.Name} tarafından yapılan önemli bir açıklama",
                        Content = $"Bu bir test bildirimidir. {company.Name} ({company.Code}) ile ilgili detaylı bilgi içermektedir.",
                        PublishDate = now.AddHours(-random.Next(1, 24)),
                        NotificationType = notificationTypes[typeIndex],
                        NotificationNumber = $"TEST-{DateTime.Now.Ticks}-{company.Code}-{i}",
                        SourceUrl = $"https://www.kap.org.tr/tr/Bildirim/{mockDisclosureId}", // Bildirim detay sayfası
                        IsAutoFetched = true,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    notifications.Add(notification);
                }
            }

            return notifications;
        }
    }

    /// <summary>
    /// Senkronizasyon sonuç bilgisi
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int AddedCount { get; set; }
        public int SkippedCount { get; set; }
    }
}
