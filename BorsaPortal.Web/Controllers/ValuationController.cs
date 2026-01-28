using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using BorsaPortal.Web.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BorsaPortal.Web.Controllers
{
    [Authorize]
    public class ValuationController : Controller
    {
        private readonly IValuationService _valuationService;
        private readonly IValuationCalculatorService _calculatorService;
        private readonly BorsaPortalDbContext _context;
        private readonly IStockPriceService _stockPriceService;
        private readonly IFinancialDataService _financialDataService;

        public ValuationController()
        {
            _context = new BorsaPortalDbContext();
            _valuationService = new ValuationService(_context);
            _calculatorService = new ValuationCalculatorService();
            _stockPriceService = new StockPriceService(_context, new PortfolioService(_context));
            _financialDataService = new FinancialDataService(_context);
        }

        // GET: Valuation
        public async Task<ActionResult> Index()
        {
            var userId = User.Identity.GetUserId();
            var valuations = await _valuationService.GetUserValuationsAsync(userId);
            var stats = await _valuationService.GetValuationStatisticsAsync(userId);

            var model = new ValuationIndexViewModel
            {
                Valuations = valuations,
                Statistics = stats
            };

            return View(model);
        }

        // GET: Valuation/Create
        public async Task<ActionResult> Create(int? companyId)
        {
            var userId = User.Identity.GetUserId();
            var canMakeValuation = await _valuationService.CanUserMakeValuationAsync(userId);

            if (!canMakeValuation)
            {
                TempData["Error"] = "Değerleme hakkınız kalmadı. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Index");
            }

            var companies = _context.Companies
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();

            var model = new CreateValuationViewModel
            {
                CompanyId = companyId,
                Companies = new SelectList(companies, "Id", "Name", companyId)
            };

            return View(model);
        }

        // GET: Valuation/GetCompanyData
        [HttpGet]
        public async Task<ActionResult> GetCompanyData(int companyId)
        {
            try
            {
                var company = await _context.Companies
                    .Include(c => c.Sector)
                    .FirstOrDefaultAsync(c => c.Id == companyId && !c.IsDeleted);

                if (company == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetCompanyData] Company not found with ID: {companyId}");
                    return Json(new { success = false, message = "Şirket bulunamadı." }, JsonRequestBehavior.AllowGet);
                }

                // Debug: Company veritabanından çekilen değerleri logla
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] Company ID: {company.Id}, Code: {company.Code}, Name: {company.Name}");
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] PaidInCapital: {company.PaidInCapital}, Equity: {company.Equity}");
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] YearlyNetProfit: {company.YearlyNetProfit}, QuarterlyNetProfit: {company.QuarterlyNetProfit}");
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] YearlySales: {company.YearlySales}, CurrentMarketValue: {company.CurrentMarketValue}");
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] PERatio: {company.PERatio}, PBRatio: {company.PBRatio}");

                // Güncel fiyatı çek
                var priceData = await _stockPriceService.GetStockPriceAsync(company.Code);
                decimal? currentPrice = priceData.IsSuccess ? priceData.CurrentPrice : (decimal?)null;

                // Not: Otomatik finansal veri çekme Türk hisse senetleri için desteklenmiyor
                // Kullanıcılar finansal verileri manuel olarak girmelidir
                // Veriler Company tablosunda mevcut olanlardır

                // BIST100 endeks verilerini çek
                var bist100 = await _context.MarketIndices
                    .FirstOrDefaultAsync(m => m.Code == "XU100" && !m.IsDeleted);

                var response = new
                {
                    success = true,
                    companyCode = company.Code,
                    companyName = company.Name,
                    currentPrice = currentPrice,
                    paidInCapital = company.PaidInCapital,
                    equity = company.Equity,
                    yearlyNetProfit = company.YearlyNetProfit,
                    quarterlyNetProfit = company.QuarterlyNetProfit,
                    yearlySales = company.YearlySales,
                    currentMarketValue = company.CurrentMarketValue,
                    stockPE = company.PERatio,
                    stockPB = company.PBRatio,
                    sectorPE = company.Sector?.AveragePE,
                    sectorPB = company.Sector?.AveragePB,
                    bist100PE = bist100?.AveragePE,
                    bist100PB = bist100?.AveragePB,
                    bist100Value = bist100?.CurrentValue,
                    netProfitYear1 = company.NetProfitYear1,
                    netProfitYear2 = company.NetProfitYear2,
                    netProfitYear3 = company.NetProfitYear3,
                    netProfitYear4 = company.NetProfitYear4,
                    equityYear1 = company.EquityYear1,
                    equityYear2 = company.EquityYear2,
                    equityYear3 = company.EquityYear3,
                    equityYear4 = company.EquityYear4,
                    peYear1 = company.PEYear1,
                    peYear2 = company.PEYear2,
                    peYear3 = company.PEYear3,
                    lastUpdate = company.LastFinancialUpdate
                };

                // Debug: Response'u logla
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] Returning success=true with {(currentPrice != null ? "price" : "no price")}");

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[GetCompanyData] StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Hata: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Valuation/Calculate
        [HttpPost]
        public async Task<ActionResult> Calculate(CreateValuationViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var canMakeValuation = await _valuationService.CanUserMakeValuationAsync(userId);

            if (!canMakeValuation)
            {
                return Json(new { success = false, message = "Değerleme hakkınız kalmadı." });
            }

            var methodResults = new List<ValuationMethodResult>();
            var allMethods = await _valuationService.GetAllValuationMethodsAsync();

            try
            {
                // Tüm yöntemleri hesapla
                foreach (var method in allMethods)
                {
                    ValuationResult result = null;

                    switch (method.MethodType)
                {
                    case "Basic":
                        result = _calculatorService.CalculateBasicValuation(new BasicValuationInput
                        {
                            YearlyNetProfit = model.YearlyNetProfit ?? 0,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            CurrentPE = model.CurrentPE ?? 0,
                            EstimatedYearlyNetProfit = model.EstimatedYearlyNetProfit ?? 0,
                            EstimatedPE = model.EstimatedPE ?? 0
                        });
                        break;

                    case "Quarter1":
                        result = _calculatorService.CalculateQuarter1Valuation(new QuarterValuationInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            QuarterNetProfit = model.QuarterNetProfit ?? 0,
                            QuarterNumber = 1,
                            Equity = model.Equity,
                            CurrentMarketValue = model.CurrentMarketValue,
                            StockPE = model.StockPE ?? 0,
                            StockPB = model.StockPB ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0,
                            SectorOrBist100PB = model.SectorPB ?? 0
                        });
                        break;

                    case "Quarter2":
                        result = _calculatorService.CalculateQuarter2Valuation(new QuarterValuationInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            QuarterNetProfit = model.QuarterNetProfit ?? 0,
                            QuarterNumber = 2,
                            Equity = model.Equity,
                            CurrentMarketValue = model.CurrentMarketValue,
                            StockPE = model.StockPE ?? 0,
                            StockPB = model.StockPB ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0,
                            SectorOrBist100PB = model.SectorPB ?? 0
                        });
                        break;

                    case "Quarter3":
                        result = _calculatorService.CalculateQuarter3Valuation(new QuarterValuationInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            QuarterNetProfit = model.QuarterNetProfit ?? 0,
                            QuarterNumber = 3,
                            Equity = model.Equity,
                            CurrentMarketValue = model.CurrentMarketValue,
                            StockPE = model.StockPE ?? 0,
                            StockPB = model.StockPB ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0,
                            SectorOrBist100PB = model.SectorPB ?? 0
                        });
                        break;

                    case "Quarter4":
                        result = _calculatorService.CalculateQuarter4Valuation(new QuarterValuationInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            QuarterNetProfit = model.YearlyNetProfit ?? 0,
                            QuarterNumber = 4,
                            Equity = model.Equity,
                            CurrentMarketValue = model.CurrentMarketValue,
                            StockPE = model.StockPE ?? 0,
                            StockPB = model.StockPB ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0,
                            SectorOrBist100PB = model.SectorPB ?? 0
                        });
                        break;

                    case "YearEnd":
                        result = _calculatorService.CalculateYearEndEstimateValuation(new YearEndEstimateInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            EstimatedYearlyNetProfit = model.EstimatedYearlyNetProfit ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0
                        });
                        break;

                    case "LongTerm":
                        result = _calculatorService.CalculateLongTermTargetValuation(new LongTermTargetInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            PaidInCapital = model.PaidInCapital ?? 0,
                            NetProfitYear1 = model.NetProfitYear1 ?? 0,
                            NetProfitYear2 = model.NetProfitYear2 ?? 0,
                            NetProfitYear3 = model.NetProfitYear3 ?? 0,
                            NetProfitYear4 = model.NetProfitYear4 ?? 0,
                            EquityYear1 = model.EquityYear1 ?? 0,
                            EquityYear2 = model.EquityYear2 ?? 0,
                            EquityYear3 = model.EquityYear3 ?? 0,
                            EquityYear4 = model.EquityYear4 ?? 0,
                            StockPE = model.StockPE ?? 0,
                            StockPB = model.StockPB ?? 0,
                            SectorOrBist100PE = model.SectorPE ?? 0
                        });
                        break;

                    case "HistoricalPE":
                        result = _calculatorService.CalculateHistoricalPEValuation(new HistoricalPEInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            CurrentPE = model.StockPE ?? 0,
                            PE_Year1 = model.PE_Year1 ?? 0,
                            PE_Year2 = model.PE_Year2 ?? 0,
                            PE_Year3 = model.PE_Year3 ?? 0
                        });
                        break;

                    case "PriceToSales":
                        result = _calculatorService.CalculatePriceToSalesValuation(new PriceToSalesInput
                        {
                            CurrentPrice = model.CurrentPrice,
                            YearlyNetProfit = model.YearlyNetProfit ?? 0,
                            YearlySales = model.YearlySales ?? 0,
                            CurrentMarketValue = model.CurrentMarketValue ?? 0
                        });
                        break;

                    default:
                        return Json(new { success = false, message = "Desteklenmeyen değerleme yöntemi." });
                }

                    // Her yöntem için sonucu kaydet
                    if (result != null)
                    {
                        var methodResult = new ValuationMethodResult
                        {
                            MethodName = method.Name,
                            MethodType = method.MethodType,
                            Success = result.Success,
                            ErrorMessage = result.Message,
                            TargetPrice = result.TargetPrice,
                            PremiumPotential = result.PremiumPotential,
                            EstimatedYearEndProfit = result.EstimatedYearEndProfit,
                            Calculations = result.Calculations?.Select(c => new ValuationCalculationDetail
                            {
                                CalculationType = c.CalculationType,
                                CalculationName = c.CalculationName,
                                CalculatedPrice = c.CalculatedPrice,
                                Formula = c.Formula,
                                FormulaWithValues = c.FormulaWithValues
                            }).ToList() ?? new List<ValuationCalculationDetail>()
                        };
                        methodResults.Add(methodResult);
                    }
                }

                // Bedelsiz potansiyelini hesapla
                decimal? bonusSharePotential = null;
                if (model.Equity.HasValue && model.PaidInCapital.HasValue && model.PaidInCapital.Value > 0)
                {
                    bonusSharePotential = _calculatorService.CalculateBonusSharePotential(
                        model.Equity.Value,
                        model.PaidInCapital.Value
                    );
                }

                // Genel ortalama hesapla (başarılı olan yöntemlerden)
                var successfulMethods = methodResults.Where(m => m.Success && m.TargetPrice.HasValue).ToList();
                decimal? overallAverageTargetPrice = null;
                decimal? overallAveragePremiumPotential = null;

                if (successfulMethods.Any())
                {
                    overallAverageTargetPrice = successfulMethods.Average(m => m.TargetPrice.Value);
                    if (model.CurrentPrice > 0)
                    {
                        overallAveragePremiumPotential = ((overallAverageTargetPrice.Value - model.CurrentPrice) / model.CurrentPrice) * 100;
                    }
                }
                else
                {
                    // Hiçbir yöntem başarılı olmadı
                    var failedReasons = methodResults
                        .Where(m => !m.Success)
                        .Select(m => $"{m.MethodName}: {m.ErrorMessage}")
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "Değerleme hesaplanamadı. Lütfen daha fazla finansal veri girin.\n\n" +
                                  "Başarısız yöntemler:\n" + string.Join("\n", failedReasons)
                    });
                }

                return Json(new
                {
                    success = true,
                    methodResults = methodResults.Select(m => new
                    {
                        methodName = m.MethodName,
                        methodType = m.MethodType,
                        success = m.Success,
                        errorMessage = m.ErrorMessage,
                        targetPrice = m.TargetPrice,
                        premiumPotential = m.PremiumPotential,
                        estimatedYearEndProfit = m.EstimatedYearEndProfit,
                        calculations = m.Calculations.Select(c => new
                        {
                            calculationType = c.CalculationType,
                            calculationName = c.CalculationName,
                            calculatedPrice = c.CalculatedPrice,
                            formula = c.Formula,
                            formulaWithValues = c.FormulaWithValues
                        })
                    }),
                    overallAverageTargetPrice = overallAverageTargetPrice,
                    overallAveragePremiumPotential = overallAveragePremiumPotential,
                    bonusSharePotential = bonusSharePotential,
                    successfulMethodsCount = successfulMethods.Count,
                    totalMethodsCount = methodResults.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // POST: Valuation/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(CreateValuationViewModel model)
        {
            var userId = User.Identity.GetUserId();
            var canMakeValuation = await _valuationService.CanUserMakeValuationAsync(userId);

            if (!canMakeValuation)
            {
                TempData["Error"] = "Değerleme hakkınız kalmadı.";
                return RedirectToAction("Index");
            }

            // CompanyId kontrolü
            if (!model.CompanyId.HasValue || model.CompanyId.Value <= 0)
            {
                TempData["Error"] = "Lütfen geçerli bir şirket seçiniz.";
                return RedirectToAction("Create");
            }

            // Önce hesaplama yapılmış mı kontrol et
            if (!model.OverallAverageTargetPrice.HasValue || model.OverallAverageTargetPrice.Value <= 0)
            {
                TempData["Error"] = "Lütfen önce değerleme hesaplaması yapınız. 'Tüm Yöntemleri Hesapla' butonuna tıklayın.";
                return RedirectToAction("Create", new { companyId = model.CompanyId });
            }

            try
            {
                var valuation = new StockValuation
                {
                    UserId = userId,
                    CompanyId = model.CompanyId.Value,
                    ValuationMethodId = null, // Tüm yöntemlerin ortalaması için null
                    CurrentPrice = model.CurrentPrice,
                    PaidInCapital = model.PaidInCapital,
                    QuarterlyNetProfit = model.QuarterNetProfit,
                    YearlyNetProfit = model.YearlyNetProfit,
                    Equity = model.Equity,
                    CurrentMarketValue = model.CurrentMarketValue,
                    StockPE = model.StockPE,
                    StockPB = model.StockPB,
                    SectorPE = model.SectorPE,
                    SectorPB = model.SectorPB,
                    NetProfitYear1 = model.NetProfitYear1,
                    NetProfitYear2 = model.NetProfitYear2,
                    NetProfitYear3 = model.NetProfitYear3,
                    NetProfitYear4 = model.NetProfitYear4,
                    EquityYear1 = model.EquityYear1,
                    EquityYear2 = model.EquityYear2,
                    EquityYear3 = model.EquityYear3,
                    EquityYear4 = model.EquityYear4,
                    PE_Year1 = model.PE_Year1,
                    PE_Year2 = model.PE_Year2,
                    PE_Year3 = model.PE_Year3,
                    YearlySales = model.YearlySales,
                    EstimatedYearEndProfit = model.EstimatedYearEndProfit,
                    TargetPrice = model.OverallAverageTargetPrice, // Genel ortalama
                    PremiumPotential = model.OverallAveragePremiumPotential, // Genel ortalama prim
                    Notes = model.Notes,
                    ValuationDate = DateTime.UtcNow
                };

                await _valuationService.CreateStockValuationAsync(valuation);
                await _valuationService.UseValuationRightAsync(userId);

                TempData["Success"] = "Değerleme başarıyla kaydedildi.";
                return RedirectToAction("Details", new { id = valuation.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Değerleme kaydedilemedi: {ex.Message}";
                return RedirectToAction("Create", new { companyId = model.CompanyId });
            }
        }

        // GET: Valuation/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var valuation = await _valuationService.GetValuationByIdAsync(id.Value);
            if (valuation == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            if (valuation.UserId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(valuation);
        }

        // POST: Valuation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var success = await _valuationService.DeleteValuationAsync(id, userId);

            if (success)
            {
                TempData["Success"] = "Değerleme başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Değerleme silinemedi.";
            }

            return RedirectToAction("Index");
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
