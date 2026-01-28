using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BorsaPortal.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public NotificationService(BorsaPortalDbContext context, IStockPriceService stockPriceService)
        {
            _context = context;
            _stockPriceService = stockPriceService;
        }

        public async Task CheckTargetPricesAndNotifyAsync()
        {
            try
            {
                // Get all watchlist items with target prices and notifications enabled
                var watchlistItems = await _context.Watchlists
                    .Include(w => w.Company)
                    .Where(w => !w.IsDeleted
                        && w.TargetPrice.HasValue
                        && w.NotifyOnTargetPrice)
                    .ToListAsync();

                if (!watchlistItems.Any())
                    return;

                // Get unique stock symbols
                var symbols = watchlistItems.Select(w => w.Company.Code).Distinct().ToList();
                var prices = await _stockPriceService.GetStockPricesAsync(symbols);

                foreach (var item in watchlistItems)
                {
                    if (!prices.TryGetValue(item.Company.Code, out var priceData) || !priceData.IsSuccess)
                        continue;

                    // Check if current price has reached or exceeded target price
                    if (priceData.CurrentPrice >= item.TargetPrice.Value)
                    {
                        // Check if notification already sent today
                        var today = DateTime.Now.Date;
                        var existingNotification = await _context.UserNotifications
                            .Where(n => n.UserId == item.UserId
                                && n.NotificationType == "TargetPrice"
                                && n.RelatedEntityId == item.Id
                                && n.NotificationDate >= today)
                            .AnyAsync();

                        if (!existingNotification)
                        {
                            var percentageReached = ((priceData.CurrentPrice - item.TargetPrice.Value) / item.TargetPrice.Value) * 100;

                            await CreateNotificationAsync(
                                item.UserId,
                                $"{item.Company.Code} Hedef Fiyata Ulaştı!",
                                $"{item.Company.Code} ({item.Company.Name}) hissesi hedef fiyatınız olan ₺{item.TargetPrice.Value:N2}'ye ulaştı. Güncel fiyat: ₺{priceData.CurrentPrice:N2} ({(percentageReached >= 0 ? "+" : "")}{percentageReached:N2}%)",
                                "TargetPrice",
                                item.Id,
                                "Watchlist",
                                $"/Watchlist/Index"
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckTargetPricesAndNotifyAsync Error: {ex.Message}");
            }
        }

        public async Task CreateNotificationAsync(
            string userId,
            string title,
            string message,
            string notificationType,
            int? relatedEntityId = null,
            string relatedEntityType = null,
            string actionUrl = null)
        {
            var notification = new UserNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = notificationType,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                ActionUrl = actionUrl,
                IsRead = false,
                NotificationDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };

            _context.UserNotifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.UserNotifications
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
