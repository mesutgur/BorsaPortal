using System;

namespace BorsaPortal.Domain.Entities
{
    public class News : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public DateTime PublishDate { get; set; }
        public string Source { get; set; }
        public string SourceUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Tags { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsAutoFetched { get; set; }
        public int ViewCount { get; set; }

        // Optional: Related to a specific company
        public int? CompanyId { get; set; }

        // Optional: Related to a specific sector
        public int? SectorId { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public virtual Sector Sector { get; set; }

        public News()
        {
            ViewCount = 0;
            PublishDate = DateTime.UtcNow;
            IsFeatured = false;
            IsAutoFetched = false;
        }
    }
}
