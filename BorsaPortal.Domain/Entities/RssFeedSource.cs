using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// RSS feed kaynağı - Haber otomasyonu için RSS feed URL'lerini saklar
    /// </summary>
    public class RssFeedSource : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FeedUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastFetchedAt { get; set; }
        public int FetchIntervalMinutes { get; set; }
        public int? SectorId { get; set; }
        public int ItemsFetchedCount { get; set; }

        // Filtering options
        public string IncludeKeywords { get; set; } // Comma-separated keywords to include
        public string ExcludeKeywords { get; set; } // Comma-separated keywords to exclude
        public bool AutoPublish { get; set; } // Otomatik yayınlansın mı

        // Navigation Properties
        public virtual Sector Sector { get; set; }

        public RssFeedSource()
        {
            IsActive = true;
            FetchIntervalMinutes = 60; // Default: her saat
            ItemsFetchedCount = 0;
            AutoPublish = false;
        }
    }
}
