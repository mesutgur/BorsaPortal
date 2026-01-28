using System;

namespace BorsaPortal.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Code { get; set; }
        public string Symbol => Code; // Finansal terminolojide Symbol kullanımı için Code'a alias
        public string Name { get; set; }
        public int? SectorId { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public decimal? PaidInCapital { get; set; }
        public string LogoUrl { get; set; }

        // Financial Data (Son finansal veriler)
        public decimal? Equity { get; set; } // Özsermaye
        public decimal? YearlyNetProfit { get; set; } // Yıllık Net Kar
        public decimal? QuarterlyNetProfit { get; set; } // Son Çeyrek Net Kar
        public decimal? YearlySales { get; set; } // Yıllık Net Satışlar
        public decimal? CurrentMarketValue { get; set; } // Güncel Piyasa Değeri

        // Calculated Ratios (Hesaplanan oranlar)
        public decimal? PERatio { get; set; } // F/K Oranı (Price/Earnings)
        public decimal? PBRatio { get; set; } // PD/DD Oranı (Price/Book)

        // Historical Financial Data (Geçmiş yıl verileri)
        public decimal? NetProfitYear1 { get; set; } // 4 yıl önce
        public decimal? NetProfitYear2 { get; set; } // 3 yıl önce
        public decimal? NetProfitYear3 { get; set; } // 2 yıl önce
        public decimal? NetProfitYear4 { get; set; } // 1 yıl önce
        public decimal? EquityYear1 { get; set; }
        public decimal? EquityYear2 { get; set; }
        public decimal? EquityYear3 { get; set; }
        public decimal? EquityYear4 { get; set; }
        public decimal? PEYear1 { get; set; } // F/K geçmişi
        public decimal? PEYear2 { get; set; }
        public decimal? PEYear3 { get; set; }

        public DateTime? LastFinancialUpdate { get; set; } // Son güncelleme tarihi

        // Navigation Properties
        public virtual Sector Sector { get; set; }
    }
}
