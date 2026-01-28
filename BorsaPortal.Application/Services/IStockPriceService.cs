using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorsaPortal.Application.Services
{
    /// <summary>
    /// Stock price data from external API
    /// </summary>
    public class StockPriceData
    {
        public string Symbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? DayChange { get; set; }
        public decimal? DayChangePercent { get; set; }
        public decimal? DayHigh { get; set; }
        public decimal? DayLow { get; set; }
        public long? Volume { get; set; }
        public long? TradeCount { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        // Convenience properties for backward compatibility
        public decimal Change => DayChange ?? 0;
        public decimal ChangePercent => DayChangePercent ?? 0;
    }

    public interface IStockPriceService
    {
        /// <summary>
        /// Gets current price for a single stock
        /// </summary>
        Task<StockPriceData> GetStockPriceAsync(string symbol);

        /// <summary>
        /// Gets current prices for multiple stocks
        /// </summary>
        Task<Dictionary<string, StockPriceData>> GetStockPricesAsync(IEnumerable<string> symbols);

        /// <summary>
        /// Updates portfolio with latest prices
        /// </summary>
        Task<bool> UpdatePortfolioPricesAsync(string userId);

        /// <summary>
        /// Gets historical price data for a stock
        /// </summary>
        Task<List<HistoricalPriceData>> GetHistoricalPricesAsync(string symbol, int days);
    }

    /// <summary>
    /// Historical price data point
    /// </summary>
    public class HistoricalPriceData
    {
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
    }
}
