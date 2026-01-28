using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BorsaPortal.Infrastructure.Services
{
    public class ValuationService : IValuationService
    {
        private readonly BorsaPortalDbContext _context;

        public ValuationService(BorsaPortalDbContext context)
        {
            _context = context;
        }

        #region Değerleme Yöntemi Yönetimi

        public async Task<List<ValuationMethod>> GetAllValuationMethodsAsync()
        {
            return await _context.ValuationMethods
                .Where(m => !m.IsDeleted && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ValuationMethod> GetValuationMethodByIdAsync(int id)
        {
            return await _context.ValuationMethods
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<ValuationMethod> GetValuationMethodByTypeAsync(string methodType)
        {
            return await _context.ValuationMethods
                .FirstOrDefaultAsync(m => m.MethodType == methodType && !m.IsDeleted && m.IsActive);
        }

        public async Task<ValuationMethod> CreateValuationMethodAsync(ValuationMethod method)
        {
            _context.ValuationMethods.Add(method);
            await _context.SaveChangesAsync();
            return method;
        }

        public async Task<ValuationMethod> UpdateValuationMethodAsync(ValuationMethod method)
        {
            method.UpdatedAt = DateTime.UtcNow;
            _context.Entry(method).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return method;
        }

        public async Task<bool> DeleteValuationMethodAsync(int id)
        {
            var method = await GetValuationMethodByIdAsync(id);
            if (method == null)
                return false;

            method.IsDeleted = true;
            method.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Kullanıcı Değerleme Hakkı Yönetimi

        public async Task<UserValuationRight> GetUserValuationRightAsync(string userId)
        {
            var right = await _context.UserValuationRights
                .FirstOrDefaultAsync(r => r.UserId == userId && !r.IsDeleted);

            if (right == null)
            {
                // İlk kez değerleme yapacak kullanıcılara varsayılan 3 hak ver
                right = new UserValuationRight
                {
                    UserId = userId,
                    RemainingValuations = 3,
                    TotalValuations = 3,
                    IsUnlimited = false
                };
                _context.UserValuationRights.Add(right);
                await _context.SaveChangesAsync();
            }

            return right;
        }

        public async Task<bool> CanUserMakeValuationAsync(string userId)
        {
            var right = await GetUserValuationRightAsync(userId);
            return right.CanMakeValuation();
        }

        public async Task<UserValuationRight> GrantValuationRightAsync(string userId, int valuationCount, int? expiryDays = null)
        {
            var right = await GetUserValuationRightAsync(userId);
            right.RemainingValuations += valuationCount;
            right.TotalValuations += valuationCount;

            if (expiryDays.HasValue)
            {
                right.ExpiryDate = DateTime.UtcNow.AddDays(expiryDays.Value);
            }

            right.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return right;
        }

        public async Task<UserValuationRight> GrantUnlimitedValuationRightAsync(string userId)
        {
            var right = await GetUserValuationRightAsync(userId);
            right.IsUnlimited = true;
            right.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return right;
        }

        public async Task UseValuationRightAsync(string userId)
        {
            var right = await GetUserValuationRightAsync(userId);
            right.UseValuation();
            right.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Hisse Değerleme İşlemleri

        public async Task<StockValuation> CreateStockValuationAsync(StockValuation valuation)
        {
            _context.StockValuations.Add(valuation);
            await _context.SaveChangesAsync();
            return valuation;
        }

        public async Task<StockValuation> GetValuationByIdAsync(int id)
        {
            return await _context.StockValuations
                .Include(v => v.Company)
                .Include(v => v.ValuationMethod)
                .Include(v => v.Calculations)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<List<StockValuation>> GetUserValuationsAsync(string userId)
        {
            return await _context.StockValuations
                .Include(v => v.Company)
                .Include(v => v.ValuationMethod)
                .Where(v => v.UserId == userId && !v.IsDeleted)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StockValuation>> GetCompanyValuationsAsync(int companyId)
        {
            return await _context.StockValuations
                .Include(v => v.User)
                .Include(v => v.ValuationMethod)
                .Where(v => v.CompanyId == companyId && !v.IsDeleted)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StockValuation>> GetUserCompanyValuationsAsync(string userId, int companyId)
        {
            return await _context.StockValuations
                .Include(v => v.ValuationMethod)
                .Include(v => v.Calculations)
                .Where(v => v.UserId == userId && v.CompanyId == companyId && !v.IsDeleted)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteValuationAsync(int id, string userId)
        {
            var valuation = await _context.StockValuations
                .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId && !v.IsDeleted);

            if (valuation == null)
                return false;

            valuation.IsDeleted = true;
            valuation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region İstatistikler

        public async Task<Dictionary<string, object>> GetValuationStatisticsAsync(string userId)
        {
            var stats = new Dictionary<string, object>();

            var userRight = await GetUserValuationRightAsync(userId);
            stats["RemainingValuations"] = userRight.IsUnlimited ? -1 : userRight.RemainingValuations;
            stats["IsUnlimited"] = userRight.IsUnlimited;

            var userValuations = await _context.StockValuations
                .Where(v => v.UserId == userId && !v.IsDeleted)
                .ToListAsync();

            var totalValuations = userValuations.Count;
            stats["TotalValuations"] = totalValuations;
            stats["TotalValuationsMade"] = totalValuations;

            var uniqueCompanies = userValuations
                .Select(v => v.CompanyId)
                .Distinct()
                .Count();
            stats["UniqueCompaniesValued"] = uniqueCompanies;

            // Ortalama prim potansiyeli hesapla
            var valuationsWithPremium = userValuations
                .Where(v => v.PremiumPotential.HasValue)
                .ToList();

            if (valuationsWithPremium.Any())
            {
                stats["AveragePremium"] = valuationsWithPremium.Average(v => v.PremiumPotential.Value);
            }
            else
            {
                stats["AveragePremium"] = 0;
            }

            // En yüksek hedef fiyat hesapla
            var valuationsWithTarget = userValuations
                .Where(v => v.TargetPrice.HasValue)
                .ToList();

            if (valuationsWithTarget.Any())
            {
                stats["HighestTarget"] = valuationsWithTarget.Max(v => v.TargetPrice.Value);
            }
            else
            {
                stats["HighestTarget"] = 0;
            }

            // Bu ayki değerleme sayısı hesapla
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlyValuations = userValuations
                .Count(v => v.CreatedAt >= monthStart);
            stats["MonthlyValuations"] = monthlyValuations;

            return stats;
        }

        #endregion
    }
}
