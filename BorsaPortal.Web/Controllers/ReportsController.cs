using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public ReportsController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: Reports
        public async Task<ActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.Identity.GetUserId();

            // Default to last 30 days if no date range specified
            var start = startDate ?? DateTime.Now.AddDays(-30);
            var end = endDate ?? DateTime.Now;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            // Get portfolio summary
            var portfolioService = new PortfolioService(_context);
            var portfolioSummary = await portfolioService.GetPortfolioSummaryAsync(userId);
            ViewBag.PortfolioSummary = portfolioSummary;

            // Get transaction history for the date range
            var transactions = await _context.PortfolioItems
                .Include(p => p.Company)
                .Include(p => p.Company.Sector)
                .Where(p => p.Portfolio.UserId == userId
                    && !p.IsDeleted
                    && p.BuyDate >= start
                    && p.BuyDate <= end)
                .OrderByDescending(p => p.BuyDate)
                .ToListAsync();
            ViewBag.Transactions = transactions;

            // Get watchlist items added in date range
            var watchlistAdditions = await _context.Watchlists
                .Include(w => w.Company)
                .Where(w => w.UserId == userId
                    && !w.IsDeleted
                    && w.AddedAt >= start
                    && w.AddedAt <= end)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
            ViewBag.WatchlistAdditions = watchlistAdditions;

            return View();
        }

        // GET: Reports/PortfolioPerformance
        public async Task<ActionResult> PortfolioPerformance(DateTime? startDate, DateTime? endDate)
        {
            var userId = User.Identity.GetUserId();

            // Default to last 90 days
            var start = startDate ?? DateTime.Now.AddDays(-90);
            var end = endDate ?? DateTime.Now;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            // Get portfolio summary
            var portfolioService = new PortfolioService(_context);
            var portfolioSummary = await portfolioService.GetPortfolioSummaryAsync(userId);
            ViewBag.PortfolioSummary = portfolioSummary;

            // Get transactions in date range
            var transactions = await _context.PortfolioItems
                .Include(p => p.Company)
                .Include(p => p.Company.Sector)
                .Where(p => p.Portfolio.UserId == userId
                    && !p.IsDeleted
                    && p.BuyDate >= start
                    && p.BuyDate <= end)
                .ToListAsync();

            // Calculate performance metrics
            var totalInvested = transactions.Sum(t => t.TotalCost);
            var totalCurrentValue = transactions.Sum(t => t.CurrentValue);
            var totalProfitLoss = totalCurrentValue - totalInvested;
            var profitLossPercentage = totalInvested > 0 ? (totalProfitLoss / totalInvested) * 100 : 0;

            ViewBag.Transactions = transactions;
            ViewBag.TotalInvested = totalInvested;
            ViewBag.TotalCurrentValue = totalCurrentValue;
            ViewBag.TotalProfitLoss = totalProfitLoss;
            ViewBag.ProfitLossPercentage = profitLossPercentage;

            // Calculate sector distribution
            var sectorDistribution = transactions
                .GroupBy(t => t.Company.Sector?.Name ?? "Diğer")
                .Select(g => new
                {
                    Sector = g.Key,
                    TotalValue = g.Sum(t => t.CurrentValue),
                    Percentage = portfolioSummary.TotalValue > 0
                        ? (g.Sum(t => t.CurrentValue) / portfolioSummary.TotalValue) * 100
                        : 0
                })
                .OrderByDescending(s => s.TotalValue)
                .ToList();
            ViewBag.SectorDistribution = sectorDistribution;

            return View();
        }

        // GET: Reports/TransactionHistory
        public async Task<ActionResult> TransactionHistory(DateTime? startDate, DateTime? endDate, string companyCode)
        {
            var userId = User.Identity.GetUserId();

            // Default to last 6 months
            var start = startDate ?? DateTime.Now.AddMonths(-6);
            var end = endDate ?? DateTime.Now;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");
            ViewBag.CompanyCode = companyCode;

            // Get transactions
            var query = _context.PortfolioItems
                .Include(p => p.Company)
                .Include(p => p.Company.Sector)
                .Where(p => p.Portfolio.UserId == userId
                    && p.BuyDate >= start
                    && p.BuyDate <= end);

            if (!string.IsNullOrEmpty(companyCode))
            {
                query = query.Where(p => p.Company.Code == companyCode);
            }

            var transactions = await query
                .OrderByDescending(p => p.BuyDate)
                .ToListAsync();

            ViewBag.Transactions = transactions;

            // Get company list for filter dropdown
            var companies = await _context.PortfolioItems
                .Include(p => p.Company)
                .Where(p => p.Portfolio.UserId == userId)
                .Select(p => p.Company.Code)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            ViewBag.Companies = companies;

            return View();
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
