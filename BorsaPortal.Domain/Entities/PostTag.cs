namespace BorsaPortal.Domain.Entities
{
    public class PostTag
    {
        public int PostId { get; set; }
        public int TagId { get; set; }

        // Navigation Properties
        public virtual BlogPost Post { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
