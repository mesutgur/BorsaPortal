using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BorsaPortal.Infrastructure.Services
{
    public class StockPriceService : IStockPriceService
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IPortfolioService _portfolioService;
        private static readonly HttpClient _httpClient;

        static StockPriceService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public StockPriceService(BorsaPortalDbContext context, IPortfolioService portfolioService)
        {
            _context = context;
            _portfolioService = portfolioService;
        }

        public async Task<StockPriceData> GetStockPriceAsync(string symbol)
        {
            try
            {
                // BIST stocks use .IS suffix for Yahoo Finance (e.g., THYAO.IS)
                var yahooSymbol = symbol.Contains(".") ? symbol : $"{symbol}.IS";

                // Yahoo Finance chart endpoint
                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{yahooSymbol}?interval=1d&range=1d";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API error for {symbol}: Status {response.StatusCode}");
                    return new StockPriceData
                    {
                        Symbol = symbol,
                        IsSuccess = false,
                        ErrorMessage = $"API Error: {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var chart = json["chart"]?["result"]?[0];
                if (chart == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API returned null chart data for {symbol}");
                    return new StockPriceData
                    {
                        Symbol = symbol,
                        IsSuccess = false,
                        ErrorMessage = "No data available"
                    };
                }

                var meta = chart["meta"];
                var quote = chart["indicators"]?["quote"]?[0];

                var currentPrice = meta?["regularMarketPrice"]?.Value<decimal?>() ?? 0;
                var previousClose = meta?["chartPreviousClose"]?.Value<decimal?>();
                var dayHigh = meta?["regularMarketDayHigh"]?.Value<decimal?>();
                var dayLow = meta?["regularMarketDayLow"]?.Value<decimal?>();
                var volume = meta?["regularMarketVolume"]?.Value<long?>();

                decimal? dayChange = null;
                decimal? dayChangePercent = null;

                if (previousClose.HasValue && previousClose.Value > 0)
                {
                    dayChange = currentPrice - previousClose.Value;
                    dayChangePercent = (dayChange.Value / previousClose.Value) * 100;
                }

                var result = new StockPriceData
                {
                    Symbol = symbol,
                    CurrentPrice = currentPrice,
                    PreviousClose = previousClose,
                    DayChange = dayChange,
                    DayChangePercent = dayChangePercent,
                    DayHigh = dayHigh,
                    DayLow = dayLow,
                    Volume = volume,
                    LastUpdate = DateTime.Now,
                    IsSuccess = true
                };

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStockPriceAsync Error for {symbol}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return new StockPriceData
                {
                    Symbol = symbol,
                    IsSuccess = false,
                    ErrorMessage = $"API Error: {ex.Message}"
                };
            }
        }

        public async Task<Dictionary<string, StockPriceData>> GetStockPricesAsync(IEnumerable<string> symbols)
        {
            var result = new Dictionary<string, StockPriceData>();

            // Process stocks sequentially with delay to avoid rate limiting
            foreach (var symbol in symbols)
            {
                var priceData = await GetStockPriceAsync(symbol);
                result[symbol] = priceData;

                // Add a small delay between requests to be respectful to Yahoo Finance
                await Task.Delay(200); // 200ms delay between requests
            }

            return result;
        }

        public async Task<bool> UpdatePortfolioPricesAsync(string userId)
        {
            try
            {
                // Get portfolio directly from context for proper tracking
                var portfolio = await _context.Portfolios
                    .Include(p => p.Items.Select(i => i.Company))
                    .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

                if (portfolio == null || !portfolio.Items.Any(i => !i.IsDeleted))
                {
                    return false;
                }

                // Get all unique stock symbols
                var symbols = portfolio.Items
                    .Where(i => !i.IsDeleted)
                    .Select(i => i.Company.Code)
                    .Distinct()
                    .ToList();

                // Fetch prices for all stocks
                var prices = await GetStockPricesAsync(symbols);

                int updatedCount = 0;
                int failedCount = 0;

                // Update each portfolio item
                foreach (var item in portfolio.Items.Where(i => !i.IsDeleted))
                {
                    if (prices.TryGetValue(item.Company.Code, out var priceData))
                    {
                        if (priceData.IsSuccess && priceData.CurrentPrice > 0)
                        {
                            item.CurrentPrice = priceData.CurrentPrice;
                            item.CurrentValue = item.Quantity * priceData.CurrentPrice;
                            item.ProfitLoss = item.CurrentValue - item.TotalCost;
                            item.ProfitLossPercentage = item.TotalCost > 0
                                ? (item.ProfitLoss / item.TotalCost) * 100
                                : 0;
                            item.LastPriceUpdate = priceData.LastUpdate;
                            updatedCount++;
                        }
                        else
                        {
                            failedCount++;
                            System.Diagnostics.Debug.WriteLine($"Failed to update {item.Company.Code}: {priceData.ErrorMessage}");
                        }
                    }
                    else
                    {
                        failedCount++;
                        System.Diagnostics.Debug.WriteLine($"No price data found for {item.Company.Code}");
                    }
                }

                if (updatedCount > 0)
                {
                    await _context.SaveChangesAsync();

                    // Update portfolio totals
                    await _portfolioService.UpdatePortfolioValuesAsync(userId);

                    System.Diagnostics.Debug.WriteLine($"UpdatePortfolioPricesAsync: Successfully updated {updatedCount} items, {failedCount} failed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePortfolioPricesAsync: No prices were updated. {failedCount} items failed.");
                }

                return updatedCount > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePortfolioPricesAsync Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<List<HistoricalPriceData>> GetHistoricalPricesAsync(string symbol, int days)
        {
            try
            {
                // BIST stocks use .IS suffix for Yahoo Finance (e.g., THYAO.IS)
                var yahooSymbol = symbol.Contains(".") ? symbol : $"{symbol}.IS";

                // Yahoo Finance chart endpoint with historical data
                // Use range parameter: 5d, 1mo, 3mo, 6mo, 1y, etc.
                var range = days <= 7 ? "5d" : days <= 35 ? "1mo" : days <= 95 ? "3mo" : days <= 185 ? "6mo" : "1y";
                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{yahooSymbol}?interval=1d&range={range}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API error for {symbol}: Status {response.StatusCode}");
                    return new List<HistoricalPriceData>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var chart = json["chart"]?["result"]?[0];
                if (chart == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API returned null chart data for {symbol}");
                    return new List<HistoricalPriceData>();
                }

                var timestamps = chart["timestamp"] as JArray;
                var quote = chart["indicators"]?["quote"]?[0];

                if (timestamps == null || quote == null)
                {
                    return new List<HistoricalPriceData>();
                }

                var opens = quote["open"] as JArray;
                var highs = quote["high"] as JArray;
                var lows = quote["low"] as JArray;
                var closes = quote["close"] as JArray;
                var volumes = quote["volume"] as JArray;

                var historicalData = new List<HistoricalPriceData>();

                for (int i = 0; i < timestamps.Count; i++)
                {
                    var timestamp = timestamps[i].Value<long>();
                    var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;

                    var close = closes?[i]?.Value<decimal?>();
                    if (!close.HasValue || close.Value <= 0)
                        continue;

                    historicalData.Add(new HistoricalPriceData
                    {
                        Date = date,
                        Open = opens?[i]?.Value<decimal?>() ?? close.Value,
                        High = highs?[i]?.Value<decimal?>() ?? close.Value,
                        Low = lows?[i]?.Value<decimal?>() ?? close.Value,
                        Close = close.Value,
                        Volume = volumes?[i]?.Value<long?>() ?? 0
                    });
                }

                return historicalData.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching historical price data for {symbol}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<HistoricalPriceData>();
            }
        }
    }
}
