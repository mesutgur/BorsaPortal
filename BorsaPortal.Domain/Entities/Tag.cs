using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }

        // Navigation Properties
        public virtual ICollection<PostTag> PostTags { get; set; }

        public Tag()
        {
            PostTags = new HashSet<PostTag>();
        }
    }
}
