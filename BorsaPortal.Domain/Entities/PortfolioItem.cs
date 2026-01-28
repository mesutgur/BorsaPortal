using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Portföy kalemi - Kullanıcının portföyündeki bir hisse
    /// </summary>
    public class PortfolioItem : BaseEntity
    {
        public int PortfolioId { get; set; }
        public int CompanyId { get; set; }
        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercentage { get; set; }
        public DateTime BuyDate { get; set; }
        public DateTime? LastPriceUpdate { get; set; }

        // Navigation Properties
        public virtual Portfolio Portfolio { get; set; }
        public virtual Company Company { get; set; }
    }
}
