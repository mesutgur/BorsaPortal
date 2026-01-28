using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcı favorileri - Haberler, KAP bildirimleri, blog yazıları
    /// </summary>
    public class UserFavorite : BaseEntity
    {
        public string UserId { get; set; }
        public string EntityType { get; set; } // "News", "KapNotification", "BlogPost"
        public int EntityId { get; set; }
        public DateTime AddedAt { get; set; }
        public string Notes { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
    }
}
