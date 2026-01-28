using System.Collections.Generic;
using System.Threading.Tasks;
using BorsaPortal.Domain.Entities;

namespace BorsaPortal.Application.Services
{
    /// <summary>
    /// Portföy yönetim servisi
    /// </summary>
    public interface IPortfolioService
    {
        /// <summary>
        /// Kullanıcının portföyünü getirir, yoksa oluşturur
        /// </summary>
        Task<Portfolio> GetOrCreatePortfolioAsync(string userId);

        /// <summary>
        /// Portföye hisse ekler (alış işlemi)
        /// </summary>
        Task<PortfolioItem> AddStockAsync(string userId, int companyId, decimal quantity, decimal price);

        /// <summary>
        /// Portföyden hisse çıkarır (satış işlemi)
        /// </summary>
        Task<bool> RemoveStockAsync(string userId, int companyId, decimal quantity, decimal price);

        /// <summary>
        /// Portföy kalemini siler
        /// </summary>
        Task<bool> DeletePortfolioItemAsync(string userId, int portfolioItemId);

        /// <summary>
        /// Portföy değerlerini günceller (fiyatlar değiştiğinde)
        /// </summary>
        Task UpdatePortfolioValuesAsync(string userId);

        /// <summary>
        /// Portföy özetini getirir
        /// </summary>
        Task<PortfolioSummary> GetPortfolioSummaryAsync(string userId);
    }

    /// <summary>
    /// Portföy özet bilgisi
    /// </summary>
    public class PortfolioSummary
    {
        // Temel Metrikler
        public int TotalCompanies { get; set; }
        public decimal TotalShares { get; set; }              // Toplam hisse adedi
        public decimal TotalValue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public decimal AverageCostPerShare { get; set; }      // Ortalama hisse maliyeti
        public decimal? DailyProfitLoss { get; set; }         // Günlük kar/zarar

        // İleri Seviye Metrikler
        public decimal TotalInvestedAmount { get; set; }  // Toplam yatırılan para
        public decimal UnrealizedProfitLoss { get; set; } // Gerçekleşmemiş kar/zarar
        public decimal DayChange { get; set; }            // Günlük değişim
        public decimal DayChangePercent { get; set; }     // Günlük değişim %
        public decimal BestPerformer { get; set; }        // En iyi performans %
        public decimal WorstPerformer { get; set; }       // En kötü performans %
        public int TotalPositions { get; set; }           // Toplam pozisyon sayısı
        public int ProfitablePositions { get; set; }      // Karlı pozisyon sayısı
        public int LossPositions { get; set; }            // Zararda pozisyon sayısı

        // Listeler
        public List<PortfolioItem> TopGainers { get; set; }
        public List<PortfolioItem> TopLosers { get; set; }
        public List<PortfolioItem> AllItems { get; set; }

        // Sektör Dağılımı
        public Dictionary<string, decimal> SectorAllocation { get; set; }
        public Dictionary<string, decimal> SectorProfitLoss { get; set; }
    }
}
