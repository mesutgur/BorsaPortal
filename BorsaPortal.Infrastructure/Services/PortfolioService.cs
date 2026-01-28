using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;

namespace BorsaPortal.Infrastructure.Services
{
    /// <summary>
    /// Portföy yönetim servisi implementasyonu
    /// </summary>
    public class PortfolioService : IPortfolioService
    {
        private readonly BorsaPortalDbContext _context;

        public PortfolioService(BorsaPortalDbContext context)
        {
            _context = context;
        }

        public async Task<Portfolio> GetOrCreatePortfolioAsync(string userId)
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.Items.Select(i => i.Company))
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

            if (portfolio == null)
            {
                portfolio = new Portfolio
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.Portfolios.Add(portfolio);
                await _context.SaveChangesAsync();
            }

            return portfolio;
        }

        public async Task<PortfolioItem> AddStockAsync(string userId, int companyId, decimal quantity, decimal price)
        {
            var portfolio = await GetOrCreatePortfolioAsync(userId);

            // Aynı şirketten mevcut bir kayıt var mı kontrol et
            var existingItem = portfolio.Items.FirstOrDefault(i => i.CompanyId == companyId && !i.IsDeleted);

            if (existingItem != null)
            {
                // Ortalama alış fiyatını hesapla
                var totalQuantity = existingItem.Quantity + quantity;
                var totalCost = existingItem.TotalCost + (quantity * price);
                var averagePrice = totalCost / totalQuantity;

                existingItem.Quantity = totalQuantity;
                existingItem.AverageBuyPrice = averagePrice;
                existingItem.TotalCost = totalCost;
                existingItem.CurrentPrice = price;
                existingItem.CurrentValue = totalQuantity * price;
                existingItem.ProfitLoss = existingItem.CurrentValue - totalCost;
                existingItem.ProfitLossPercentage = totalCost > 0 ? (existingItem.ProfitLoss / totalCost) * 100 : 0;
                existingItem.LastPriceUpdate = DateTime.Now;
            }
            else
            {
                // Yeni portföy kalemi oluştur
                var newItem = new PortfolioItem
                {
                    PortfolioId = portfolio.Id,
                    CompanyId = companyId,
                    Quantity = quantity,
                    AverageBuyPrice = price,
                    TotalCost = quantity * price,
                    CurrentPrice = price,
                    CurrentValue = quantity * price,
                    ProfitLoss = 0,
                    ProfitLossPercentage = 0,
                    BuyDate = DateTime.Now,
                    LastPriceUpdate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                _context.PortfolioItems.Add(newItem);
                existingItem = newItem;
            }

            await _context.SaveChangesAsync();
            await UpdatePortfolioTotalsAsync(portfolio);

            return existingItem;
        }

        public async Task<bool> RemoveStockAsync(string userId, int companyId, decimal quantity, decimal price)
        {
            var portfolio = await GetOrCreatePortfolioAsync(userId);
            var item = portfolio.Items.FirstOrDefault(i => i.CompanyId == companyId && !i.IsDeleted);

            if (item == null)
                return false;

            if (item.Quantity <= quantity)
            {
                // Tüm hisseyi sat - soft delete
                item.IsDeleted = true;
            }
            else
            {
                // Kısmi satış
                var remainingQuantity = item.Quantity - quantity;
                var soldCost = (item.TotalCost / item.Quantity) * quantity;

                item.Quantity = remainingQuantity;
                item.TotalCost -= soldCost;
                item.CurrentValue = remainingQuantity * price;
                item.ProfitLoss = item.CurrentValue - item.TotalCost;
                item.ProfitLossPercentage = item.TotalCost > 0 ? (item.ProfitLoss / item.TotalCost) * 100 : 0;
                item.LastPriceUpdate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            await UpdatePortfolioTotalsAsync(portfolio);

            return true;
        }

        public async Task<bool> DeletePortfolioItemAsync(string userId, int portfolioItemId)
        {
            var portfolio = await GetOrCreatePortfolioAsync(userId);
            var item = portfolio.Items.FirstOrDefault(i => i.Id == portfolioItemId && !i.IsDeleted);

            if (item == null)
                return false;

            item.IsDeleted = true;
            await _context.SaveChangesAsync();
            await UpdatePortfolioTotalsAsync(portfolio);

            return true;
        }

        public async Task UpdatePortfolioValuesAsync(string userId)
        {
            var portfolio = await GetOrCreatePortfolioAsync(userId);

            foreach (var item in portfolio.Items.Where(i => !i.IsDeleted))
            {
                // Burada gerçek API'den fiyat çekilebilir
                // Şimdilik mevcut fiyatı kullanıyoruz
                item.CurrentValue = item.Quantity * item.CurrentPrice;
                item.ProfitLoss = item.CurrentValue - item.TotalCost;
                item.ProfitLossPercentage = item.TotalCost > 0 ? (item.ProfitLoss / item.TotalCost) * 100 : 0;
            }

            await _context.SaveChangesAsync();
            await UpdatePortfolioTotalsAsync(portfolio);
        }

        public async Task<PortfolioSummary> GetPortfolioSummaryAsync(string userId)
        {
            var portfolio = await GetOrCreatePortfolioAsync(userId);
            var activeItems = portfolio.Items.Where(i => !i.IsDeleted).ToList();

            var totalShares = activeItems.Sum(i => i.Quantity);

            var summary = new PortfolioSummary
            {
                // Temel Metrikler
                TotalCompanies = activeItems.Count,
                TotalShares = totalShares,
                TotalValue = activeItems.Sum(i => i.CurrentValue),
                TotalCost = activeItems.Sum(i => i.TotalCost),
                TotalProfitLoss = activeItems.Sum(i => i.ProfitLoss),
                AllItems = activeItems.OrderByDescending(i => i.CurrentValue).ToList(),
                AverageCostPerShare = totalShares > 0 ? activeItems.Sum(i => i.TotalCost) / totalShares : 0,
                DailyProfitLoss = null, // Historical data not available yet

                // İleri Seviye Metrikler
                TotalInvestedAmount = activeItems.Sum(i => i.TotalCost),
                UnrealizedProfitLoss = activeItems.Sum(i => i.ProfitLoss),
                TotalPositions = activeItems.Count,
                ProfitablePositions = activeItems.Count(i => i.ProfitLoss > 0),
                LossPositions = activeItems.Count(i => i.ProfitLoss < 0),
                BestPerformer = activeItems.Any() ? activeItems.Max(i => i.ProfitLossPercentage) : 0,
                WorstPerformer = activeItems.Any() ? activeItems.Min(i => i.ProfitLossPercentage) : 0
            };

            // Yüzde hesaplamaları
            summary.TotalProfitLossPercentage = summary.TotalCost > 0
                ? (summary.TotalProfitLoss / summary.TotalCost) * 100
                : 0;

            // En iyi ve en kötü performans gösterenler
            summary.TopGainers = activeItems
                .Where(i => i.ProfitLoss > 0)
                .OrderByDescending(i => i.ProfitLossPercentage)
                .Take(5)
                .ToList();

            summary.TopLosers = activeItems
                .Where(i => i.ProfitLoss < 0)
                .OrderBy(i => i.ProfitLossPercentage)
                .Take(5)
                .ToList();

            // Sektör Dağılımı Hesaplaması
            summary.SectorAllocation = new Dictionary<string, decimal>();
            summary.SectorProfitLoss = new Dictionary<string, decimal>();

            var sectorGroups = activeItems.GroupBy(i => i.Company.Sector.Name);
            foreach (var group in sectorGroups)
            {
                var sectorValue = group.Sum(i => i.CurrentValue);
                var sectorProfitLoss = group.Sum(i => i.ProfitLoss);
                var sectorPercentage = summary.TotalValue > 0
                    ? (sectorValue / summary.TotalValue) * 100
                    : 0;

                summary.SectorAllocation[group.Key] = sectorPercentage;
                summary.SectorProfitLoss[group.Key] = sectorProfitLoss;
            }

            return summary;
        }

        private async Task UpdatePortfolioTotalsAsync(Portfolio portfolio)
        {
            var activeItems = portfolio.Items.Where(i => !i.IsDeleted).ToList();

            portfolio.TotalValue = activeItems.Sum(i => i.CurrentValue);
            portfolio.TotalCost = activeItems.Sum(i => i.TotalCost);
            portfolio.TotalProfitLoss = activeItems.Sum(i => i.ProfitLoss);
            portfolio.TotalProfitLossPercentage = portfolio.TotalCost > 0
                ? (portfolio.TotalProfitLoss / portfolio.TotalCost) * 100
                : 0;

            await _context.SaveChangesAsync();
        }
    }
}
