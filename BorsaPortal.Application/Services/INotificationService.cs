using System.Threading.Tasks;

namespace BorsaPortal.Application.Services
{
    public interface INotificationService
    {
        Task CheckTargetPricesAndNotifyAsync();
        Task CreateNotificationAsync(string userId, string title, string message, string notificationType, int? relatedEntityId = null, string relatedEntityType = null, string actionUrl = null);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
