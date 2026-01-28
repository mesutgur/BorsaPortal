using BorsaPortal.Domain.Entities;
using System.Collections.Generic;

namespace BorsaPortal.Application.Services
{
    public interface IValuationCalculatorService
    {
        /// <summary>
        /// Temel değer biçme hesaplaması yapar
        /// HBK = Yıllık Net Kar / Ödenmiş Sermaye
        /// Hisse Fiyat = HBK × F/K Oranı
        /// </summary>
        ValuationResult CalculateBasicValuation(BasicValuationInput input);

        /// <summary>
        /// 1. Çeyrek bilançoya göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateQuarter1Valuation(QuarterValuationInput input);

        /// <summary>
        /// 2. Çeyrek bilançoya göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateQuarter2Valuation(QuarterValuationInput input);

        /// <summary>
        /// 3. Çeyrek bilançoya göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateQuarter3Valuation(QuarterValuationInput input);

        /// <summary>
        /// 4. Çeyrek (Yıllık) bilançoya göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateQuarter4Valuation(QuarterValuationInput input);

        /// <summary>
        /// Yıl sonu tahmini kara göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateYearEndEstimateValuation(YearEndEstimateInput input);

        /// <summary>
        /// Uzun vade hedef hesaplama (geçmiş performans bazlı)
        /// </summary>
        ValuationResult CalculateLongTermTargetValuation(LongTermTargetInput input);

        /// <summary>
        /// Tarihsel F/K oranına göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculateHistoricalPEValuation(HistoricalPEInput input);

        /// <summary>
        /// PD/NS (Piyasa Değeri / Net Satış) yöntemine göre hisse değeri hesaplar
        /// </summary>
        ValuationResult CalculatePriceToSalesValuation(PriceToSalesInput input);

        /// <summary>
        /// Bedelsiz hisse potansiyelini hesaplar
        /// </summary>
        decimal CalculateBonusSharePotential(decimal equity, decimal paidInCapital);
    }

    #region Input Models

    public class BasicValuationInput
    {
        public decimal YearlyNetProfit { get; set; }
        public decimal PaidInCapital { get; set; }
        public decimal CurrentPE { get; set; }
        public decimal EstimatedYearlyNetProfit { get; set; }
        public decimal EstimatedPE { get; set; }
    }

    public class QuarterValuationInput
    {
        public string StockSymbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PaidInCapital { get; set; }
        public decimal QuarterNetProfit { get; set; } // Çeyrek net kar
        public int QuarterNumber { get; set; } // 1, 2, 3, veya 4
        public decimal? Equity { get; set; }
        public decimal? CurrentMarketValue { get; set; }
        public decimal StockPE { get; set; }
        public decimal StockPB { get; set; }
        public decimal SectorOrBist100PE { get; set; }
        public decimal SectorOrBist100PB { get; set; }
    }

    public class YearEndEstimateInput
    {
        public string StockSymbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PaidInCapital { get; set; }
        public decimal EstimatedYearlyNetProfit { get; set; }
        public decimal SectorOrBist100PE { get; set; }
    }

    public class LongTermTargetInput
    {
        public string StockSymbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal PaidInCapital { get; set; }
        public decimal NetProfitYear1 { get; set; }  // En eski yıl
        public decimal NetProfitYear2 { get; set; }
        public decimal NetProfitYear3 { get; set; }
        public decimal NetProfitYear4 { get; set; }  // En yeni yıl
        public decimal EquityYear1 { get; set; }     // En eski yıl
        public decimal EquityYear2 { get; set; }
        public decimal EquityYear3 { get; set; }
        public decimal EquityYear4 { get; set; }     // En yeni yıl
        public decimal StockPE { get; set; }
        public decimal StockPB { get; set; }
        public decimal SectorOrBist100PE { get; set; }
    }

    public class HistoricalPEInput
    {
        public string StockSymbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal CurrentPE { get; set; }
        public decimal PE_Year1 { get; set; }  // En eski yıl
        public decimal PE_Year2 { get; set; }
        public decimal PE_Year3 { get; set; }  // En yeni yıl
    }

    public class PriceToSalesInput
    {
        public string StockSymbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal YearlyNetProfit { get; set; }
        public decimal YearlySales { get; set; }
        public decimal CurrentMarketValue { get; set; }
    }

    #endregion

    #region Result Models

    public class ValuationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal? EstimatedYearEndProfit { get; set; }
        public decimal? TargetPrice { get; set; }
        public decimal? AverageTargetPrice { get; set; }
        public decimal? PremiumPotential { get; set; } // Prim potansiyeli (%)
        public decimal? BonusSharePotential { get; set; } // Bedelsiz potansiyeli (%)
        public List<ValuationCalculationResult> Calculations { get; set; }

        public ValuationResult()
        {
            Calculations = new List<ValuationCalculationResult>();
        }
    }

    public class ValuationCalculationResult
    {
        public string CalculationType { get; set; }
        public string CalculationName { get; set; }
        public decimal? CalculatedPrice { get; set; }
        public string Formula { get; set; }
        public string FormulaWithValues { get; set; }
    }

    #endregion
}
