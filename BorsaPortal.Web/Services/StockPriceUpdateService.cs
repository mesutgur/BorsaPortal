using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using BorsaPortal.Web.Hubs;
using Microsoft.AspNet.SignalR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BorsaPortal.Web.Services
{
    public class StockPriceUpdateService
    {
        private static Timer _marketDataTimer;
        private static readonly object _lock = new object();
        private static bool _isRunning = false;

        public static void Start()
        {
            lock (_lock)
            {
                if (_isRunning)
                    return;

                // Update market data every 30 seconds
                _marketDataTimer = new Timer(async (state) =>
                {
                    await UpdateMarketDataAsync();
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

                _isRunning = true;
                System.Diagnostics.Debug.WriteLine("StockPriceUpdateService started");
            }
        }

        public static void Stop()
        {
            lock (_lock)
            {
                _marketDataTimer?.Dispose();
                _isRunning = false;
                System.Diagnostics.Debug.WriteLine("StockPriceUpdateService stopped");
            }
        }

        private static async Task UpdateMarketDataAsync()
        {
            try
            {
                var marketDataService = new MarketDataService();
                var marketSummary = await marketDataService.GetMarketSummaryAsync();

                if (marketSummary != null && marketSummary.IsSuccess)
                {
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<StockPriceHub>();
                    await hubContext.Clients.Group("market_data").ReceiveMarketData(marketSummary);

                    System.Diagnostics.Debug.WriteLine($"Market data updated at {DateTime.Now:HH:mm:ss}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating market data: {ex.Message}");
            }
        }

        public static async Task UpdateStockPriceAsync(string symbol)
        {
            try
            {
                using (var context = new BorsaPortalDbContext())
                {
                    var portfolioService = new PortfolioService(context);
                    var stockPriceService = new StockPriceService(context, portfolioService);

                    var priceData = await stockPriceService.GetStockPriceAsync(symbol);

                    if (priceData != null && priceData.IsSuccess)
                    {
                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<StockPriceHub>();
                        await hubContext.Clients.Group($"stock_{symbol}").ReceiveStockPrice(symbol, priceData);

                        System.Diagnostics.Debug.WriteLine($"Stock price updated for {symbol} at {DateTime.Now:HH:mm:ss}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stock price for {symbol}: {ex.Message}");
            }
        }
    }
}
