using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcı portföyü - Kullanıcının sahip olduğu hisselerin toplamı
    /// </summary>
    public class Portfolio : BaseEntity
    {
        public string UserId { get; set; }
        public decimal TotalValue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<PortfolioItem> Items { get; set; }

        public Portfolio()
        {
            Items = new HashSet<PortfolioItem>();
        }
    }
}
