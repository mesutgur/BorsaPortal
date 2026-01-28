using System;
using System.Collections.Generic;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcının bir hisse için yaptığı değerleme kaydı
    /// </summary>
    public class StockValuation : BaseEntity
    {
        public string UserId { get; set; }
        public int CompanyId { get; set; }
        public int? ValuationMethodId { get; set; } // Nullable - genel ortalama için null olabilir

        // Input Parametreleri
        public decimal CurrentPrice { get; set; }
        public decimal? PaidInCapital { get; set; }
        public decimal? QuarterlyNetProfit { get; set; } // Çeyreklik kar
        public decimal? SixMonthNetProfit { get; set; } // 6 aylık kar
        public decimal? NineMonthNetProfit { get; set; } // 9 aylık kar
        public decimal? YearlyNetProfit { get; set; } // Yıllık kar
        public decimal? Equity { get; set; } // Özsermaye
        public decimal? CurrentMarketValue { get; set; }
        public decimal? StockPE { get; set; } // Hisse F/K oranı
        public decimal? StockPB { get; set; } // Hisse PD/DD oranı
        public decimal? SectorPE { get; set; } // Sektör/BIST100 F/K oranı
        public decimal? SectorPB { get; set; } // Sektör/BIST100 PD/DD oranı

        // Uzun vade hesaplamaları için geçmiş yıl verileri (Year1=En eski, Year4=En yeni)
        public decimal? NetProfitYear1 { get; set; }
        public decimal? NetProfitYear2 { get; set; }
        public decimal? NetProfitYear3 { get; set; }
        public decimal? NetProfitYear4 { get; set; }
        public decimal? EquityYear1 { get; set; }
        public decimal? EquityYear2 { get; set; }
        public decimal? EquityYear3 { get; set; }
        public decimal? EquityYear4 { get; set; }

        // Tarihsel F/K için geçmiş yıl F/K oranları (Year1=En eski, Year3=En yeni)
        public decimal? PE_Year1 { get; set; }
        public decimal? PE_Year2 { get; set; }
        public decimal? PE_Year3 { get; set; }

        // PD/NS yöntemi için satış verileri
        public decimal? YearlySales { get; set; } // Yıllık net satış

        // Hesaplanan Değerler
        public decimal? EstimatedYearEndProfit { get; set; }
        public decimal? TargetPrice { get; set; }
        public decimal? PremiumPotential { get; set; } // Prim potansiyeli (%)

        public string Notes { get; set; }
        public DateTime ValuationDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }
        public virtual Company Company { get; set; }
        public virtual ValuationMethod ValuationMethod { get; set; }
        public virtual ICollection<ValuationCalculation> Calculations { get; set; }

        public StockValuation()
        {
            ValuationDate = DateTime.UtcNow;
            Calculations = new HashSet<ValuationCalculation>();
        }
    }
}
