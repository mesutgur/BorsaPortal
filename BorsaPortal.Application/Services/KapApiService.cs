using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BorsaPortal.Application.Services
{
    public class KapApiService : IKapApiService
    {
        private readonly HttpClient _httpClient;
        private const string KAP_API_BASE_URL = "https://www.kap.org.tr/tr/api";

        public KapApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BorsaPortal/1.0");
        }

        /// <summary>
        /// Belirtilen tarih aralığındaki KAP bildirimlerini çeker
        /// </summary>
        public async Task<List<KapNotification>> FetchNotificationsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // KAP API endpoint: /disclosures
                var url = $"{KAP_API_BASE_URL}/disclosures";

                // Query parameters
                var parameters = new Dictionary<string, string>
                {
                    { "fromDate", startDate.ToString("yyyy-MM-dd") },
                    { "toDate", endDate.ToString("yyyy-MM-dd") }
                };

                var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"));
                var fullUrl = $"{url}?{queryString}";

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return new List<KapNotification>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var notifications = ParseKapResponse(content);

                return notifications;
            }
            catch (Exception)
            {
                // Log the error in production
                return new List<KapNotification>();
            }
        }

        /// <summary>
        /// Belirli bir şirketin KAP bildirimlerini çeker
        /// </summary>
        public async Task<List<KapNotification>> FetchNotificationsByCompanyAsync(string companyCode, DateTime startDate, DateTime endDate)
        {
            var allNotifications = await FetchNotificationsAsync(startDate, endDate);

            // Filter by company code (KAP'ta şirket kodu ile eşleştirme yapılır)
            return allNotifications
                .Where(n => n.Company?.Code?.Equals(companyCode, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        /// <summary>
        /// Son N gündeki tüm KAP bildirimlerini çeker
        /// </summary>
        public async Task<List<KapNotification>> FetchRecentNotificationsAsync(int days = 1)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-days);

            return await FetchNotificationsAsync(startDate, endDate);
        }

        /// <summary>
        /// KAP API response'unu parse eder
        /// </summary>
        private List<KapNotification> ParseKapResponse(string jsonContent)
        {
            var notifications = new List<KapNotification>();

            try
            {
                var json = JObject.Parse(jsonContent);
                var disclosures = json["disclosures"] as JArray;

                if (disclosures == null)
                    return notifications;

                foreach (var item in disclosures)
                {
                    try
                    {
                        var notification = new KapNotification
                        {
                            Title = item["title"]?.ToString(),
                            Summary = item["summary"]?.ToString(),
                            Content = item["disclosureContent"]?.ToString(),
                            PublishDate = ParseDate(item["publishDate"]?.ToString()),
                            NotificationNumber = item["disclosureIndex"]?.ToString(),
                            SourceUrl = $"https://www.kap.org.tr/tr/Bildirim/{item["disclosureIndex"]?.ToString()}",
                            IsAutoFetched = true,
                            CreatedAt = DateTime.Now,
                            IsDeleted = false,
                            NotificationType = ParseNotificationType(item["disclosureType"]?.ToString())
                        };

                        // CompanyId mapping will be done in the service layer that uses this
                        notifications.Add(notification);
                    }
                    catch
                    {
                        // Skip invalid items
                        continue;
                    }
                }
            }
            catch
            {
                // Return empty list if parsing fails
            }

            return notifications;
        }

        /// <summary>
        /// Tarih string'ini DateTime'a çevirir
        /// </summary>
        private DateTime ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
                return date;

            return DateTime.Now;
        }

        /// <summary>
        /// KAP bildirim türünü enum'a map eder
        /// </summary>
        private KapNotificationType ParseNotificationType(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
                return KapNotificationType.Other;

            // KAP'tan gelen bildirim türüne göre mapping
            typeString = typeString.ToLower();

            if (typeString.Contains("mali") || typeString.Contains("finansal"))
                return KapNotificationType.FinancialStatement;

            if (typeString.Contains("genel kurul"))
                return KapNotificationType.GeneralAssembly;

            if (typeString.Contains("ortaklık") || typeString.Contains("yapı"))
                return KapNotificationType.OwnershipStructure;

            if (typeString.Contains("özel durum"))
                return KapNotificationType.MaterialDisclosure;

            if (typeString.Contains("kar payı") || typeString.Contains("temettü"))
                return KapNotificationType.Dividend;

            if (typeString.Contains("yönetim kurulu"))
                return KapNotificationType.BoardDecision;

            if (typeString.Contains("sermaye artır"))
                return KapNotificationType.CapitalIncrease;

            if (typeString.Contains("birleşme") || typeString.Contains("devralma"))
                return KapNotificationType.MergerAcquisition;

            if (typeString.Contains("ilişkili taraf"))
                return KapNotificationType.Related;

            return KapNotificationType.Other;
        }
    }
}
