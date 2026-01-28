using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BorsaPortal.Infrastructure.Services
{
    public class FinancialDataService : IFinancialDataService
    {
        private readonly BorsaPortalDbContext _context;
        private static readonly HttpClient _httpClient;
        private static readonly string _twelveDataApiKey;
        private const string TWELVE_DATA_BASE_URL = "https://api.twelvedata.com";

        static FinancialDataService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BorsaPortal/1.0");

            // Twelve Data API Key'i önce environment variable'dan, sonra config'den al
            // Environment variable: TWELVE_DATA_API_KEY
            _twelveDataApiKey = Environment.GetEnvironmentVariable("TWELVE_DATA_API_KEY")
                ?? ConfigurationManager.AppSettings["TwelveDataApiKey"]
                ?? "demo";
        }

        public FinancialDataService(BorsaPortalDbContext context)
        {
            _context = context;
        }

        public async Task<CompanyFinancialData> GetCompanyFinancialsAsync(string symbol)
        {
            try
            {
                // BIST stocks use :BIST suffix for Twelve Data (e.g., THYAO:BIST)
                var twelveDataSymbol = symbol.Contains(":") ? symbol : $"{symbol}:BIST";

                var financials = new CompanyFinancialData
                {
                    Symbol = symbol,
                    IsSuccess = false
                };

                // 1. Get Quote data (price, market cap)
                var quoteUrl = $"{TWELVE_DATA_BASE_URL}/quote?symbol={twelveDataSymbol}&apikey={_twelveDataApiKey}";
                var quoteResponse = await _httpClient.GetAsync(quoteUrl);

                if (quoteResponse.IsSuccessStatusCode)
                {
                    var quoteContent = await quoteResponse.Content.ReadAsStringAsync();
                    var quoteJson = JObject.Parse(quoteContent);

                    // Check for error
                    if (quoteJson["status"]?.ToString() == "error")
                    {
                        return new CompanyFinancialData
                        {
                            Symbol = symbol,
                            IsSuccess = false,
                            ErrorMessage = quoteJson["message"]?.ToString() ?? "Quote data unavailable"
                        };
                    }

                    financials.CurrentPrice = ParseDecimal(quoteJson["close"]);
                }

                // 2. Get Statistics data (P/E, P/B, market cap, etc.)
                var statsUrl = $"{TWELVE_DATA_BASE_URL}/statistics?symbol={twelveDataSymbol}&apikey={_twelveDataApiKey}";
                var statsResponse = await _httpClient.GetAsync(statsUrl);

                if (statsResponse.IsSuccessStatusCode)
                {
                    var statsContent = await statsResponse.Content.ReadAsStringAsync();
                    var statsJson = JObject.Parse(statsContent);

                    // Check for error
                    if (statsJson["status"]?.ToString() != "error")
                    {
                        var stats = statsJson["statistics"];
                        var valuations = stats?["valuations_metrics"];
                        var financialMetrics = stats?["financials"];

                        // Market Cap
                        financials.MarketCap = ParseDecimal(valuations?["market_capitalization"]);

                        // Ratios
                        financials.PERatio = ParseDecimal(valuations?["trailing_pe"])
                                          ?? ParseDecimal(valuations?["forward_pe"]);
                        financials.PBRatio = ParseDecimal(valuations?["price_to_book_ratio"]);

                        // Profit Margin
                        financials.ProfitMargin = ParseDecimal(financialMetrics?["profit_margin"]);

                        // EPS
                        financials.EarningsPerShare = ParseDecimal(valuations?["earnings_per_share"]);
                        financials.BookValuePerShare = ParseDecimal(valuations?["book_value_per_share"]);

                        financials.IsSuccess = true;
                    }
                }

                // 3. Get Income Statement (for revenue and net income)
                var incomeUrl = $"{TWELVE_DATA_BASE_URL}/income_statement?symbol={twelveDataSymbol}&apikey={_twelveDataApiKey}";
                var incomeResponse = await _httpClient.GetAsync(incomeUrl);

                if (incomeResponse.IsSuccessStatusCode)
                {
                    var incomeContent = await incomeResponse.Content.ReadAsStringAsync();
                    var incomeJson = JObject.Parse(incomeContent);

                    // Check for error
                    if (incomeJson["status"]?.ToString() != "error")
                    {
                        // Get most recent annual data
                        var annualReports = incomeJson["income_statement"] as JArray;
                        if (annualReports != null && annualReports.Count > 0)
                        {
                            var latestReport = annualReports[0];
                            financials.TotalRevenue = ParseDecimal(latestReport["total_revenue"]);
                            financials.NetIncome = ParseDecimal(latestReport["net_income"]);
                            financials.IsSuccess = true;
                        }
                    }
                }

                // 4. Get Balance Sheet (for equity)
                var balanceUrl = $"{TWELVE_DATA_BASE_URL}/balance_sheet?symbol={twelveDataSymbol}&apikey={_twelveDataApiKey}";
                var balanceResponse = await _httpClient.GetAsync(balanceUrl);

                if (balanceResponse.IsSuccessStatusCode)
                {
                    var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
                    var balanceJson = JObject.Parse(balanceContent);

                    // Check for error
                    if (balanceJson["status"]?.ToString() != "error")
                    {
                        // Get most recent data
                        var balanceReports = balanceJson["balance_sheet"] as JArray;
                        if (balanceReports != null && balanceReports.Count > 0)
                        {
                            var latestReport = balanceReports[0];
                            financials.TotalEquity = ParseDecimal(latestReport["total_stockholder_equity"])
                                                  ?? ParseDecimal(latestReport["total_equity"]);
                            financials.IsSuccess = true;
                        }
                    }
                }

                // If we got at least some data, consider it a success
                if (!financials.IsSuccess)
                {
                    return new CompanyFinancialData
                    {
                        Symbol = symbol,
                        IsSuccess = false,
                        ErrorMessage = "Finansal veriler API'den çekilemedi. API anahtarınızı kontrol edin veya farklı bir hisse senedi deneyin."
                    };
                }

                return financials;
            }
            catch (Exception ex)
            {
                return new CompanyFinancialData
                {
                    Symbol = symbol,
                    IsSuccess = false,
                    ErrorMessage = $"Hata: {ex.Message}"
                };
            }
        }

        public async Task<CompanyFinancialData> UpdateCompanyFinancialsAsync(int companyId)
        {
            try
            {
                var company = await _context.Companies
                    .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);

                if (company == null)
                {
                    return new CompanyFinancialData
                    {
                        IsSuccess = false,
                        ErrorMessage = "Şirket bulunamadı."
                    };
                }

                var financials = await GetCompanyFinancialsAsync(company.Code);

                if (!financials.IsSuccess)
                    return financials;

                // Update Company with financial data
                company.CurrentMarketValue = financials.MarketCap;
                company.YearlySales = financials.TotalRevenue;
                // Note: Yahoo Finance doesn't provide quarterly data easily
                company.YearlyNetProfit = financials.NetIncome;
                company.PERatio = financials.PERatio;
                company.PBRatio = financials.PBRatio;

                // Calculate Equity if we have Book Value Per Share and can estimate shares
                if (financials.BookValuePerShare.HasValue && financials.MarketCap.HasValue && financials.CurrentPrice.HasValue && financials.CurrentPrice.Value > 0)
                {
                    var estimatedShares = financials.MarketCap.Value / financials.CurrentPrice.Value;
                    company.Equity = financials.BookValuePerShare.Value * estimatedShares;
                }

                company.LastFinancialUpdate = DateTime.UtcNow;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return financials;
            }
            catch (Exception ex)
            {
                return new CompanyFinancialData
                {
                    IsSuccess = false,
                    ErrorMessage = $"Güncelleme hatası: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Twelve Data JSON değerini decimal'e çevirir
        /// </summary>
        private decimal? ParseDecimal(JToken token)
        {
            if (token == null)
                return null;

            // Try direct decimal value
            var directValue = token.Value<decimal?>();
            if (directValue.HasValue)
                return directValue;

            // Try parsing string value
            var stringValue = token.Value<string>();
            if (!string.IsNullOrWhiteSpace(stringValue))
            {
                // Remove common formatting characters
                stringValue = stringValue.Replace(",", "").Replace("%", "").Trim();

                if (decimal.TryParse(stringValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }

            return null;
        }
    }
}
