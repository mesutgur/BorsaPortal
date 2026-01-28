using BorsaPortal.Domain.Enums;
using System;
using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    public class BlogPost : BaseEntity
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Body { get; set; }
        public string Summary { get; set; }
        public string FeaturedImageUrl { get; set; }

        // SEO Fields
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string SeoKeywords { get; set; }

        // Publishing
        public PublishStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string AuthorId { get; set; }

        // Category
        public int CategoryId { get; set; }

        // Stats
        public int ViewCount { get; set; }
        public int CommentCount { get; set; }

        // Navigation Properties
        public virtual BlogCategory Category { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public BlogPost()
        {
            PostTags = new HashSet<PostTag>();
            Comments = new HashSet<Comment>();
            Status = PublishStatus.Draft;
            ViewCount = 0;
            CommentCount = 0;
        }
    }
}
