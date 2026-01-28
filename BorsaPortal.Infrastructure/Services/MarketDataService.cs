using BorsaPortal.Application.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BorsaPortal.Infrastructure.Services
{
    public class MarketDataService
    {
        private static readonly HttpClient _httpClient;

        static MarketDataService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<MarketSummaryData> GetMarketSummaryAsync()
        {
            var summary = new MarketSummaryData();

            try
            {
                // BIST 100
                var bist100 = await GetYahooFinanceDataAsync("XU100.IS");
                if (bist100 != null)
                {
                    summary.Bist100 = new MarketIndexData
                    {
                        Name = "BIST 100",
                        Symbol = "XU100.IS",
                        Value = bist100.CurrentPrice,
                        Change = bist100.DayChange ?? 0,
                        ChangePercent = bist100.DayChangePercent ?? 0,
                        IsSuccess = true
                    };
                }

                // USD/TRY
                var usdtry = await GetYahooFinanceDataAsync("USDTRY=X");
                if (usdtry != null)
                {
                    summary.UsdTry = new CurrencyData
                    {
                        Name = "Dolar",
                        Symbol = "USD/TRY",
                        Buy = usdtry.CurrentPrice,
                        Sell = usdtry.CurrentPrice,
                        Change = usdtry.DayChange ?? 0,
                        ChangePercent = usdtry.DayChangePercent ?? 0,
                        IsSuccess = true
                    };
                }

                // EUR/TRY
                var eurtry = await GetYahooFinanceDataAsync("EURTRY=X");
                if (eurtry != null)
                {
                    summary.EurTry = new CurrencyData
                    {
                        Name = "Euro",
                        Symbol = "EUR/TRY",
                        Buy = eurtry.CurrentPrice,
                        Sell = eurtry.CurrentPrice,
                        Change = eurtry.DayChange ?? 0,
                        ChangePercent = eurtry.DayChangePercent ?? 0,
                        IsSuccess = true
                    };
                }

                // Altın (Gram) - Ons altın fiyatından hesaplama
                var goldOunce = await GetYahooFinanceDataAsync("GC=F"); // Gold Futures
                if (goldOunce != null && usdtry != null)
                {
                    // 1 ons = 31.1035 gram
                    // Gram altın (TRY) = (Ons altın (USD) * USD/TRY) / 31.1035
                    decimal gramGoldPrice = (goldOunce.CurrentPrice * usdtry.CurrentPrice) / 31.1035m;
                    decimal previousGramPrice = goldOunce.PreviousClose.HasValue && usdtry.PreviousClose.HasValue
                        ? (goldOunce.PreviousClose.Value * usdtry.PreviousClose.Value) / 31.1035m
                        : gramGoldPrice;

                    decimal gramChange = gramGoldPrice - previousGramPrice;
                    decimal gramChangePercent = previousGramPrice > 0 ? (gramChange / previousGramPrice) * 100 : 0;

                    summary.Gold = new CommodityData
                    {
                        Name = "Gram Altın",
                        Symbol = "XAU",
                        Price = gramGoldPrice,
                        Change = gramChange,
                        ChangePercent = gramChangePercent,
                        IsSuccess = true
                    };
                }

                summary.LastUpdate = DateTime.Now;
                summary.IsSuccess = true;
            }
            catch (Exception ex)
            {
                summary.IsSuccess = false;
                summary.ErrorMessage = ex.Message;
            }

            return summary;
        }

        private async Task<StockPriceData> GetYahooFinanceDataAsync(string symbol)
        {
            try
            {
                var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d&range=1d";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API error for {symbol}: Status {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var chart = json["chart"]?["result"]?[0];
                if (chart == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Yahoo Finance API returned null chart data for {symbol}");
                    return null;
                }

                var meta = chart["meta"];
                var quote = chart["indicators"]?["quote"]?[0];

                var currentPrice = meta?["regularMarketPrice"]?.Value<decimal?>() ?? 0;
                var previousClose = meta?["chartPreviousClose"]?.Value<decimal?>();

                decimal? dayChange = null;
                decimal? dayChangePercent = null;

                if (previousClose.HasValue && previousClose.Value > 0)
                {
                    dayChange = currentPrice - previousClose.Value;
                    dayChangePercent = (dayChange.Value / previousClose.Value) * 100;
                }

                return new StockPriceData
                {
                    Symbol = symbol,
                    CurrentPrice = currentPrice,
                    PreviousClose = previousClose,
                    DayChange = dayChange,
                    DayChangePercent = dayChangePercent,
                    LastUpdate = DateTime.Now,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching Yahoo Finance data for {symbol}: {ex.Message}");
                // Log the full exception for debugging
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }

    public class MarketSummaryData
    {
        public MarketIndexData Bist100 { get; set; }
        public CurrencyData UsdTry { get; set; }
        public CurrencyData EurTry { get; set; }
        public CommodityData Gold { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class MarketIndexData
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal Value { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class CurrencyData
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal Buy { get; set; }
        public decimal Sell { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class CommodityData
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public bool IsSuccess { get; set; }
    }
}
