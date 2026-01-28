using BorsaPortal.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorsaPortal.Application.Services
{
    public interface IValuationService
    {
        // Değerleme yöntemi yönetimi
        Task<List<ValuationMethod>> GetAllValuationMethodsAsync();
        Task<ValuationMethod> GetValuationMethodByIdAsync(int id);
        Task<ValuationMethod> GetValuationMethodByTypeAsync(string methodType);
        Task<ValuationMethod> CreateValuationMethodAsync(ValuationMethod method);
        Task<ValuationMethod> UpdateValuationMethodAsync(ValuationMethod method);
        Task<bool> DeleteValuationMethodAsync(int id);

        // Kullanıcı değerleme hakkı yönetimi
        Task<UserValuationRight> GetUserValuationRightAsync(string userId);
        Task<bool> CanUserMakeValuationAsync(string userId);
        Task<UserValuationRight> GrantValuationRightAsync(string userId, int valuationCount, int? expiryDays = null);
        Task<UserValuationRight> GrantUnlimitedValuationRightAsync(string userId);
        Task UseValuationRightAsync(string userId);

        // Hisse değerleme işlemleri
        Task<StockValuation> CreateStockValuationAsync(StockValuation valuation);
        Task<StockValuation> GetValuationByIdAsync(int id);
        Task<List<StockValuation>> GetUserValuationsAsync(string userId);
        Task<List<StockValuation>> GetCompanyValuationsAsync(int companyId);
        Task<List<StockValuation>> GetUserCompanyValuationsAsync(string userId, int companyId);
        Task<bool> DeleteValuationAsync(int id, string userId);

        // İstatistikler
        Task<Dictionary<string, object>> GetValuationStatisticsAsync(string userId);
    }
}
