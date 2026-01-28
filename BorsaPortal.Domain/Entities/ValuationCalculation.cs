namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Bir değerleme içerisindeki farklı hesaplama yöntemlerinin sonuçlarını tutar
    /// Örnek: "Cari F/K'ya göre", "Future F/K'ya göre", "PD/DD'ye göre" vb.
    /// </summary>
    public class ValuationCalculation : BaseEntity
    {
        public int StockValuationId { get; set; }
        public string CalculationType { get; set; } // "CurrentPE", "FuturePE", "PB", "PaidCapital", "PotentialMarketValue", "ROE"
        public string CalculationName { get; set; } // "Cari F/K Oranına Göre Olması Gereken Fiyat"
        public decimal? CalculatedPrice { get; set; }
        public string Formula { get; set; } // Kullanılan formül
        public string FormulaWithValues { get; set; } // Değerlerle birlikte formül

        // Navigation Properties
        public virtual StockValuation StockValuation { get; set; }
    }
}
