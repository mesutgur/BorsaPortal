namespace BorsaPortal.Domain.Entities
{
    public class MarketIndex : BaseEntity
    {
        public string Code { get; set; } // XU100, XU030, etc.
        public string Name { get; set; } // BIST 100, BIST 30, etc.
        public string Description { get; set; }

        // Index Metrics
        public decimal? CurrentValue { get; set; } // Güncel endeks değeri
        public decimal? AveragePE { get; set; } // Endeks Ortalama F/K Oranı
        public decimal? AveragePB { get; set; } // Endeks Ortalama PD/DD Oranı
        public decimal? DailyChange { get; set; } // Günlük değişim yüzdesi
        public decimal? TotalMarketCap { get; set; } // Toplam piyasa değeri
    }
}
