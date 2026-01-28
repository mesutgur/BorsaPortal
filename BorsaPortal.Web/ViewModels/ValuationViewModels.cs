using BorsaPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BorsaPortal.Web.ViewModels
{
    public class ValuationIndexViewModel
    {
        public List<StockValuation> Valuations { get; set; }
        public Dictionary<string, object> Statistics { get; set; }
    }

    public class CreateValuationViewModel
    {
        [Required(ErrorMessage = "Şirket seçmelisiniz")]
        public int? CompanyId { get; set; }

        [Required(ErrorMessage = "Güncel fiyat gereklidir")]
        [Display(Name = "Güncel Hisse Fiyatı")]
        public decimal CurrentPrice { get; set; }

        [Display(Name = "Ödenmiş Sermaye")]
        public decimal? PaidInCapital { get; set; }

        [Display(Name = "Çeyreklik Net Kar")]
        public decimal? QuarterNetProfit { get; set; }

        [Display(Name = "6 Aylık Net Kar")]
        public decimal? SixMonthNetProfit { get; set; }

        [Display(Name = "9 Aylık Net Kar")]
        public decimal? NineMonthNetProfit { get; set; }

        [Display(Name = "Yıllık Net Kar")]
        public decimal? YearlyNetProfit { get; set; }

        [Display(Name = "Özsermaye")]
        public decimal? Equity { get; set; }

        [Display(Name = "Güncel Piyasa Değeri")]
        public decimal? CurrentMarketValue { get; set; }

        [Display(Name = "Hisse F/K Oranı")]
        public decimal? StockPE { get; set; }

        [Display(Name = "Hisse PD/DD Oranı")]
        public decimal? StockPB { get; set; }

        [Display(Name = "Sektör/BIST100 F/K Oranı")]
        public decimal? SectorPE { get; set; }

        [Display(Name = "Sektör/BIST100 PD/DD Oranı")]
        public decimal? SectorPB { get; set; }

        [Display(Name = "Güncel F/K Oranı")]
        public decimal? CurrentPE { get; set; }

        [Display(Name = "Tahmini F/K Oranı")]
        public decimal? EstimatedPE { get; set; }

        [Display(Name = "Tahmini Yıllık Net Kar")]
        public decimal? EstimatedYearlyNetProfit { get; set; }

        [Display(Name = "Tahmini Yıl Sonu Karı")]
        public decimal? EstimatedYearEndProfit { get; set; }

        // Uzun vade için geçmiş yıl verileri (Year1=En eski, Year4=En yeni)
        [Display(Name = "Net Kar (Yıl 1)")]
        public decimal? NetProfitYear1 { get; set; }

        [Display(Name = "Net Kar (Yıl 2)")]
        public decimal? NetProfitYear2 { get; set; }

        [Display(Name = "Net Kar (Yıl 3)")]
        public decimal? NetProfitYear3 { get; set; }

        [Display(Name = "Net Kar (Yıl 4)")]
        public decimal? NetProfitYear4 { get; set; }

        [Display(Name = "Özsermaye (Yıl 1)")]
        public decimal? EquityYear1 { get; set; }

        [Display(Name = "Özsermaye (Yıl 2)")]
        public decimal? EquityYear2 { get; set; }

        [Display(Name = "Özsermaye (Yıl 3)")]
        public decimal? EquityYear3 { get; set; }

        [Display(Name = "Özsermaye (Yıl 4)")]
        public decimal? EquityYear4 { get; set; }

        // Tarihsel F/K için (Year1=En eski, Year3=En yeni)
        [Display(Name = "F/K Oranı (Yıl 1)")]
        public decimal? PE_Year1 { get; set; }

        [Display(Name = "F/K Oranı (Yıl 2)")]
        public decimal? PE_Year2 { get; set; }

        [Display(Name = "F/K Oranı (Yıl 3)")]
        public decimal? PE_Year3 { get; set; }

        // PD/NS Yöntemi için
        [Display(Name = "Yıllık Net Satış")]
        public decimal? YearlySales { get; set; }

        [Display(Name = "Hedef Fiyat")]
        public decimal? TargetPrice { get; set; }

        [Display(Name = "Prim Potansiyeli (%)")]
        public decimal? PremiumPotential { get; set; }

        [Display(Name = "Bedelsiz Potansiyeli (%)")]
        public decimal? BonusSharePotential { get; set; }

        [Display(Name = "Notlar")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public SelectList Companies { get; set; }

        // Tüm yöntemlerin sonuçları
        public List<ValuationMethodResult> MethodResults { get; set; }

        // Genel ortalama hedef fiyat (tüm yöntemlerin ortalaması)
        public decimal? OverallAverageTargetPrice { get; set; }

        // Genel ortalama prim potansiyeli
        public decimal? OverallAveragePremiumPotential { get; set; }

        // Dinamik yıl hesaplamaları (bugünkü yıldan geriye doğru)
        public int GetYear1() => DateTime.Now.Year - 3;  // En eski yıl
        public int GetYear2() => DateTime.Now.Year - 2;
        public int GetYear3() => DateTime.Now.Year - 1;
        public int GetYear4() => DateTime.Now.Year - 1;  // En yeni tamamlanmış yıl (çoğunlukla geçen yıl)

        public int GetHistoricalPEYear1() => DateTime.Now.Year - 3;  // En eski (tarihsel F/K için)
        public int GetHistoricalPEYear2() => DateTime.Now.Year - 2;
        public int GetHistoricalPEYear3() => DateTime.Now.Year - 1;  // En yeni

        public CreateValuationViewModel()
        {
            MethodResults = new List<ValuationMethodResult>();
        }
    }

    // Her yöntem için sonuç yapısı
    public class ValuationMethodResult
    {
        public string MethodName { get; set; }
        public string MethodType { get; set; }
        public decimal? TargetPrice { get; set; }
        public decimal? PremiumPotential { get; set; }
        public decimal? EstimatedYearEndProfit { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<ValuationCalculationDetail> Calculations { get; set; }

        public ValuationMethodResult()
        {
            Calculations = new List<ValuationCalculationDetail>();
        }
    }

    // Hesaplama detayları
    public class ValuationCalculationDetail
    {
        public string CalculationType { get; set; }
        public string CalculationName { get; set; }
        public decimal? CalculatedPrice { get; set; }
        public string Formula { get; set; }
        public string FormulaWithValues { get; set; }
    }
}
