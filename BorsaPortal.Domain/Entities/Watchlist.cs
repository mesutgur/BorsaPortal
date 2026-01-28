using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// İzleme listesi - Kullanıcının takip ettiği şirketler
    /// </summary>
    public class Watchlist : BaseEntity
    {
        public string UserId { get; set; }
        public int CompanyId { get; set; }
        public string Notes { get; set; }
        public DateTime AddedAt { get; set; }
        public decimal? TargetPrice { get; set; }
        public bool NotifyOnTargetPrice { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual Company Company { get; set; }
    }
}
