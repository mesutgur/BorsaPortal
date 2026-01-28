using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcı bildirimleri - Hedef fiyat, KAP bildirimleri vb.
    /// </summary>
    public class UserNotification : BaseEntity
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; } // TargetPrice, KAP, News, System
        public int? RelatedEntityId { get; set; } // CompanyId, WatchlistId, etc.
        public string RelatedEntityType { get; set; } // Company, Watchlist, etc.
        public bool IsRead { get; set; }
        public DateTime NotificationDate { get; set; }
        public string ActionUrl { get; set; } // Link to related page

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
    }
}
