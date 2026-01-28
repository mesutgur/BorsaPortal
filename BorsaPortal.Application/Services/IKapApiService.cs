using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BorsaPortal.Domain.Entities;

namespace BorsaPortal.Application.Services
{
    public interface IKapApiService
    {
        /// <summary>
        /// Belirtilen tarih aralığındaki KAP bildirimlerini çeker
        /// </summary>
        Task<List<KapNotification>> FetchNotificationsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Belirli bir şirketin KAP bildirimlerini çeker
        /// </summary>
        Task<List<KapNotification>> FetchNotificationsByCompanyAsync(string companyCode, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Son N gündeki tüm KAP bildirimlerini çeker
        /// </summary>
        Task<List<KapNotification>> FetchRecentNotificationsAsync(int days = 1);
    }
}
