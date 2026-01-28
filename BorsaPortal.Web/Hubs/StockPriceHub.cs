using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;

namespace BorsaPortal.Web.Hubs
{
    public class StockPriceHub : Hub
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public StockPriceHub()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        public async Task SubscribeToStock(string symbol)
        {
            try
            {
                await Groups.Add(Context.ConnectionId, $"stock_{symbol}");

                // Send initial price data
                var priceData = await _stockPriceService.GetStockPriceAsync(symbol);
                if (priceData != null && priceData.IsSuccess)
                {
                    await Clients.Caller.ReceiveStockPrice(symbol, priceData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to stock {symbol}: {ex.Message}");
                // Notify client about the error
                await Clients.Caller.ReceiveStockPrice(symbol, new
                {
                    IsSuccess = false,
                    ErrorMessage = $"Hisse verisi alınamadı: {symbol}",
                    Symbol = symbol
                });
            }
        }

        public async Task UnsubscribeFromStock(string symbol)
        {
            try
            {
                await Groups.Remove(Context.ConnectionId, $"stock_{symbol}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unsubscribing from stock {symbol}: {ex.Message}");
            }
        }

        public async Task SubscribeToMarketData()
        {
            try
            {
                await Groups.Add(Context.ConnectionId, "market_data");

                // Send initial market data
                var marketDataService = new MarketDataService();
                var marketSummary = await marketDataService.GetMarketSummaryAsync();

                if (marketSummary != null && marketSummary.IsSuccess)
                {
                    await Clients.Caller.ReceiveMarketData(marketSummary);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error subscribing to market data: {ex.Message}");
                // Notify client about the error
                await Clients.Caller.ReceiveMarketData(new
                {
                    IsSuccess = false,
                    ErrorMessage = "Piyasa verileri alınamadı."
                });
            }
        }

        public async Task UnsubscribeFromMarketData()
        {
            try
            {
                await Groups.Remove(Context.ConnectionId, "market_data");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error unsubscribing from market data: {ex.Message}");
            }
        }

        public override Task OnConnected()
        {
            System.Diagnostics.Debug.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            System.Diagnostics.Debug.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnected(stopCalled);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
