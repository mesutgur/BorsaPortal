using BorsaPortal.Domain.Enums;
using System;

namespace BorsaPortal.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Text { get; set; }
        public PublishStatus Status { get; set; }

        // User Info
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserEmail { get; set; }

        // Related Entity (polymorphic)
        public string EntityType { get; set; } // "BlogPost", "News", etc.
        public int EntityId { get; set; }

        // Blog Post relation (for when EntityType = "BlogPost")
        public int? BlogPostId { get; set; }

        // Moderation
        public DateTime? ModeratedAt { get; set; }
        public string ModeratedBy { get; set; }
        public string ModerationNotes { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual BlogPost BlogPost { get; set; }

        public Comment()
        {
            Status = PublishStatus.Pending;
        }
    }
}
