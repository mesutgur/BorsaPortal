using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BorsaPortal.Application.Services;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using Microsoft.AspNet.Identity;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class AdvancedAnalyticsController : Controller
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;

        public AdvancedAnalyticsController()
        {
            _context = new BorsaPortalDbContext();
            var portfolioService = new PortfolioService(_context);
            _stockPriceService = new StockPriceService(_context, portfolioService);
        }

        // GET: AdvancedAnalytics
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var portfolioService = new PortfolioService(_context);

            // Get portfolio summary
            var portfolioSummary = await portfolioService.GetPortfolioSummaryAsync(userId);
            ViewBag.PortfolioSummary = portfolioSummary;

            // Calculate diversification metrics
            var diversification = CalculateDiversification(portfolioSummary);
            ViewBag.Diversification = diversification;

            // Calculate risk metrics
            var riskMetrics = CalculateRiskMetrics(portfolioSummary);
            ViewBag.RiskMetrics = riskMetrics;

            // Sector concentration
            var sectorConcentration = portfolioSummary.AllItems
                .GroupBy(i => i.Company.Sector?.Name ?? "Diğer")
                .Select(g => new
                {
                    Sector = g.Key,
                    Value = g.Sum(i => i.CurrentValue),
                    Count = g.Count(),
                    Percentage = portfolioSummary.TotalValue > 0 ? (g.Sum(i => i.CurrentValue) / portfolioSummary.TotalValue) * 100 : 0
                })
                .OrderByDescending(s => s.Value)
                .ToList();
            ViewBag.SectorConcentration = sectorConcentration;

            // Top holdings
            var topHoldings = portfolioSummary.AllItems
                .OrderByDescending(i => i.CurrentValue)
                .Take(10)
                .Select(i => new
                {
                    Code = i.Company.Code,
                    Name = i.Company.Name,
                    Value = i.CurrentValue,
                    Percentage = portfolioSummary.TotalValue > 0 ? (i.CurrentValue / portfolioSummary.TotalValue) * 100 : 0,
                    ProfitLoss = i.ProfitLoss,
                    ProfitLossPercentage = i.ProfitLossPercentage
                })
                .ToList();
            ViewBag.TopHoldings = topHoldings;

            return View();
        }

        // GET: AdvancedAnalytics/Comparison
        public async Task<ActionResult> Comparison()
        {
            var userId = User.Identity.GetUserId();

            // Get user's portfolio companies for dropdown
            var companies = await _context.PortfolioItems
                .Include(p => p.Company)
                .Where(p => p.Portfolio.UserId == userId && !p.IsDeleted)
                .Select(p => p.Company)
                .Distinct()
                .OrderBy(c => c.Code)
                .ToListAsync();

            ViewBag.Companies = companies;

            return View();
        }

        // POST: AdvancedAnalytics/CompareStocks
        [HttpPost]
        public async Task<ActionResult> CompareStocks(int[] companyIds)
        {
            if (companyIds == null || companyIds.Length < 2)
            {
                TempData["ErrorMessage"] = "Lütfen karşılaştırmak için en az 2 hisse seçiniz.";
                return RedirectToAction("Comparison");
            }

            if (companyIds.Length > 5)
            {
                TempData["ErrorMessage"] = "Maksimum 5 hisse karşılaştırılabilir.";
                return RedirectToAction("Comparison");
            }

            var userId = User.Identity.GetUserId();

            // Get portfolio items for selected companies
            var portfolioItems = await _context.PortfolioItems
                .Include(p => p.Company)
                .Include(p => p.Company.Sector)
                .Where(p => p.Portfolio.UserId == userId
                    && companyIds.Contains(p.Company.Id)
                    && !p.IsDeleted)
                .ToListAsync();

            // Group by company and calculate totals
            var comparisonData = portfolioItems
                .GroupBy(p => p.Company)
                .Select(g => new
                {
                    Company = g.Key,
                    TotalQuantity = g.Sum(p => p.Quantity),
                    TotalCost = g.Sum(p => p.TotalCost),
                    TotalValue = g.Sum(p => p.CurrentValue),
                    AverageBuyPrice = g.Average(p => p.AverageBuyPrice),
                    CurrentPrice = g.First().CurrentPrice,
                    ProfitLoss = g.Sum(p => p.ProfitLoss),
                    ProfitLossPercentage = g.Sum(p => p.TotalCost) > 0
                        ? ((g.Sum(p => p.CurrentValue) - g.Sum(p => p.TotalCost)) / g.Sum(p => p.TotalCost)) * 100
                        : 0
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();

            ViewBag.ComparisonData = comparisonData;

            // Get all companies for dropdown
            var companies = await _context.PortfolioItems
                .Include(p => p.Company)
                .Where(p => p.Portfolio.UserId == userId && !p.IsDeleted)
                .Select(p => p.Company)
                .Distinct()
                .OrderBy(c => c.Code)
                .ToListAsync();
            ViewBag.Companies = companies;

            return View("Comparison");
        }

        // GET: AdvancedAnalytics/PerformanceTrends
        public async Task<ActionResult> PerformanceTrends(int months = 3)
        {
            var userId = User.Identity.GetUserId();
            var startDate = DateTime.Now.AddMonths(-months);

            // Get transactions grouped by month
            var transactions = await _context.PortfolioItems
                .Include(p => p.Company)
                .Where(p => p.Portfolio.UserId == userId
                    && !p.IsDeleted
                    && p.BuyDate >= startDate)
                .ToListAsync();

            var monthlyData = transactions
                .GroupBy(t => new { Year = t.BuyDate.Year, Month = t.BuyDate.Month })
                .Select(g => new
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:00}",
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalInvested = g.Sum(t => t.TotalCost),
                    TotalValue = g.Sum(t => t.CurrentValue),
                    ProfitLoss = g.Sum(t => t.ProfitLoss),
                    Count = g.Count()
                })
                .OrderBy(m => m.Date)
                .ToList();

            ViewBag.MonthlyData = monthlyData;
            ViewBag.SelectedMonths = months;

            // Get portfolio summary
            var portfolioService = new PortfolioService(_context);
            var portfolioSummary = await portfolioService.GetPortfolioSummaryAsync(userId);
            ViewBag.PortfolioSummary = portfolioSummary;

            return View();
        }

        private Dictionary<string, object> CalculateDiversification(PortfolioSummary summary)
        {
            if (summary.AllItems == null || !summary.AllItems.Any())
                return new Dictionary<string, object>();

            // Herfindahl-Hirschman Index (HHI) - Market concentration measure
            var totalValue = summary.TotalValue;
            var hhi = summary.AllItems
                .Sum(i => Math.Pow((double)(i.CurrentValue / totalValue * 100), 2));

            // Number of holdings
            var numberOfHoldings = summary.AllItems.Count;

            // Sector diversity
            var numberOfSectors = summary.AllItems
                .Select(i => i.Company.Sector?.Name ?? "Diğer")
                .Distinct()
                .Count();

            // Concentration of top 5 holdings
            var top5Concentration = summary.AllItems
                .OrderByDescending(i => i.CurrentValue)
                .Take(5)
                .Sum(i => i.CurrentValue) / totalValue * 100;

            // Diversification score (0-100, higher is better)
            var diversificationScore = CalculateDiversificationScore(hhi, numberOfHoldings, numberOfSectors, (decimal)top5Concentration);

            return new Dictionary<string, object>
            {
                { "HHI", hhi },
                { "NumberOfHoldings", numberOfHoldings },
                { "NumberOfSectors", numberOfSectors },
                { "Top5Concentration", top5Concentration },
                { "DiversificationScore", diversificationScore }
            };
        }

        private decimal CalculateDiversificationScore(double hhi, int holdings, int sectors, decimal top5Concentration)
        {
            decimal score = 0;

            // HHI component (0-30 points) - lower HHI is better
            if (hhi < 1000) score += 30;
            else if (hhi < 1500) score += 25;
            else if (hhi < 2500) score += 15;
            else score += 5;

            // Number of holdings (0-30 points)
            if (holdings >= 20) score += 30;
            else if (holdings >= 15) score += 25;
            else if (holdings >= 10) score += 20;
            else if (holdings >= 5) score += 10;
            else score += 5;

            // Sector diversity (0-20 points)
            if (sectors >= 8) score += 20;
            else if (sectors >= 6) score += 15;
            else if (sectors >= 4) score += 10;
            else score += 5;

            // Top 5 concentration (0-20 points) - lower is better
            if (top5Concentration < 30) score += 20;
            else if (top5Concentration < 50) score += 15;
            else if (top5Concentration < 70) score += 10;
            else score += 5;

            return score;
        }

        private Dictionary<string, object> CalculateRiskMetrics(PortfolioSummary summary)
        {
            if (summary.AllItems == null || !summary.AllItems.Any())
                return new Dictionary<string, object>();

            // Calculate volatility (standard deviation of returns)
            var returns = summary.AllItems.Select(i => i.ProfitLossPercentage).ToList();
            var avgReturn = returns.Average();
            var variance = returns.Sum(r => Math.Pow((double)(r - avgReturn), 2)) / returns.Count;
            var volatility = Math.Sqrt(variance);

            // Calculate max drawdown (worst loss)
            var maxLoss = summary.AllItems.Min(i => i.ProfitLossPercentage);

            // Calculate Sharpe-like ratio (simplified, assuming risk-free rate = 0)
            var sharpeRatio = volatility > 0 ? (double)avgReturn / volatility : 0;

            // Risk level based on volatility
            string riskLevel;
            if (volatility < 10) riskLevel = "Düşük";
            else if (volatility < 20) riskLevel = "Orta";
            else if (volatility < 30) riskLevel = "Yüksek";
            else riskLevel = "Çok Yüksek";

            return new Dictionary<string, object>
            {
                { "Volatility", volatility },
                { "AverageReturn", avgReturn },
                { "MaxDrawdown", maxLoss },
                { "SharpeRatio", sharpeRatio },
                { "RiskLevel", riskLevel }
            };
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
