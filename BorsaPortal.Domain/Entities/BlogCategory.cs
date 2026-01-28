using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    public class BlogCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public virtual ICollection<BlogPost> Posts { get; set; }

        public BlogCategory()
        {
            Posts = new HashSet<BlogPost>();
        }
    }
}
