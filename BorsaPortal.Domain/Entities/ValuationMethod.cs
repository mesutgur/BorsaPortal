using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Değerleme yöntemlerini tanımlar (Excel tablolarındaki farklı hesaplama yöntemleri)
    /// </summary>
    public class ValuationMethod : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MethodType { get; set; } // "Quarter1", "Quarter2", "Quarter3", "Quarter4", "YearEnd", "LongTerm", "Basic"
        public string Formula { get; set; } // Formül açıklaması
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public virtual ICollection<StockValuation> StockValuations { get; set; }

        public ValuationMethod()
        {
            IsActive = true;
            StockValuations = new HashSet<StockValuation>();
        }
    }
}
