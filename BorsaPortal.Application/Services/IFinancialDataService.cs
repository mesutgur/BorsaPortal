using System.Threading.Tasks;

namespace BorsaPortal.Application.Services
{
    public interface IFinancialDataService
    {
        /// <summary>
        /// Şirketin finansal verilerini Yahoo Finance'den çeker
        /// </summary>
        Task<CompanyFinancialData> GetCompanyFinancialsAsync(string symbol);

        /// <summary>
        /// Şirketin güncel finansal verilerini Company nesnesine doldurur
        /// </summary>
        Task<CompanyFinancialData> UpdateCompanyFinancialsAsync(int companyId);
    }

    public class CompanyFinancialData
    {
        public string Symbol { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        // Basic Info
        public decimal? MarketCap { get; set; } // Piyasa Değeri
        public decimal? CurrentPrice { get; set; }

        // Financial Metrics
        public decimal? TotalRevenue { get; set; } // Toplam Gelir (Yıllık Satışlar)
        public decimal? NetIncome { get; set; } // Net Kar
        public decimal? TotalEquity { get; set; } // Özsermaye

        // Ratios
        public decimal? PERatio { get; set; } // F/K Oranı
        public decimal? PBRatio { get; set; } // PD/DD Oranı
        public decimal? ProfitMargin { get; set; } // Kar Marjı

        // Per Share
        public decimal? EarningsPerShare { get; set; } // Hisse Başına Kazanç
        public decimal? BookValuePerShare { get; set; } // Hisse Başına Defter Değeri
    }
}
