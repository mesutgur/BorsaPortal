using BorsaPortal.Domain.Enums;
using System;

namespace BorsaPortal.Domain.Entities
{
    public class KapNotification : BaseEntity
    {
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public KapNotificationType NotificationType { get; set; }
        public string NotificationNumber { get; set; }
        public string SourceUrl { get; set; }
        public bool IsAutoFetched { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }

        public KapNotification()
        {
            IsAutoFetched = false;
        }
    }
}
