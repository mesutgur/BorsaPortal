using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using BorsaPortal.Infrastructure.Services;
using BorsaPortal.Web.Areas.Admin.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.Controllers
{
    public class CompanyController : AdminBaseController
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IFinancialDataService _financialDataService;

        public CompanyController()
        {
            _context = new BorsaPortalDbContext();
            _financialDataService = new FinancialDataService(_context);
        }

        // GET: Admin/Company
        public ActionResult Index()
        {
            var companies = _context.Companies
                .Include(c => c.Sector)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToList();

            return View(companies);
        }

        // GET: Admin/Company/Create
        public ActionResult Create()
        {
            var model = new CompanyViewModel
            {
                Sectors = GetSectorSelectList()
            };

            return View(model);
        }

        // POST: Admin/Company/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyViewModel model)
        {
            // Debug: Gelen model değerlerini logla
            System.Diagnostics.Debug.WriteLine($"[Company Create POST] Code: {model.Code}, Name: {model.Name}");
            System.Diagnostics.Debug.WriteLine($"[Company Create POST] PaidInCapital: {model.PaidInCapital}, Equity: {model.Equity}");
            System.Diagnostics.Debug.WriteLine($"[Company Create POST] YearlyNetProfit: {model.YearlyNetProfit}, QuarterlyNetProfit: {model.QuarterlyNetProfit}");
            System.Diagnostics.Debug.WriteLine($"[Company Create POST] ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                // Model geçersizse, hataları detaylı logla ve kullanıcıya göster
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] ModelState is INVALID. Errors:");
                var errorMessages = new System.Text.StringBuilder();
                errorMessages.AppendLine("Form doğrulama hataları:");

                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        var errorMsg = $"Alan: {modelState.Key}, Hata: {error.ErrorMessage}";
                        System.Diagnostics.Debug.WriteLine($"  - {errorMsg}");
                        errorMessages.AppendLine($"• {modelState.Key}: {error.ErrorMessage}");
                    }
                }

                SetErrorMessage(errorMessages.ToString());
                model.Sectors = GetSectorSelectList();
                return View(model);
            }

            try
            {
                var company = new Company
                {
                    Code = model.Code,
                    Name = model.Name,
                    SectorId = model.SectorId,
                    Description = model.Description,
                    Website = model.Website,
                    PaidInCapital = model.PaidInCapital,
                    LogoUrl = model.LogoUrl,
                    Equity = model.Equity,
                    YearlyNetProfit = model.YearlyNetProfit,
                    QuarterlyNetProfit = model.QuarterlyNetProfit,
                    YearlySales = model.YearlySales,
                    CurrentMarketValue = model.CurrentMarketValue,
                    PERatio = model.PERatio,
                    PBRatio = model.PBRatio,
                    NetProfitYear1 = model.NetProfitYear1,
                    NetProfitYear2 = model.NetProfitYear2,
                    NetProfitYear3 = model.NetProfitYear3,
                    NetProfitYear4 = model.NetProfitYear4,
                    EquityYear1 = model.EquityYear1,
                    EquityYear2 = model.EquityYear2,
                    EquityYear3 = model.EquityYear3,
                    EquityYear4 = model.EquityYear4,
                    PEYear1 = model.PEYear1,
                    PEYear2 = model.PEYear2,
                    PEYear3 = model.PEYear3,
                    LastFinancialUpdate = model.LastFinancialUpdate,
                    CreatedAt = DateTime.UtcNow
                };

                System.Diagnostics.Debug.WriteLine($"[Company Create POST] Adding company to context...");
                _context.Companies.Add(company);

                System.Diagnostics.Debug.WriteLine($"[Company Create POST] Calling SaveChanges()...");
                var affectedRows = _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] SaveChanges() completed. Affected rows: {affectedRows}");

                if (affectedRows > 0)
                {
                    SetSuccessMessage($"{model.Name} şirketi başarıyla oluşturuldu. ({affectedRows} kayıt eklendi)");
                    System.Diagnostics.Debug.WriteLine($"[Company Create POST] SUCCESS: Created company with {affectedRows} rows");
                }
                else
                {
                    SetSuccessMessage($"{model.Name} şirketi kaydedildi ancak değişiklik tespit edilmedi.");
                    System.Diagnostics.Debug.WriteLine($"[Company Create POST] WARNING: SaveChanges returned 0 rows");
                }

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // Entity validation hatalarını detaylı logla
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] DbEntityValidationException:");
                var errorDetails = new System.Text.StringBuilder();
                errorDetails.AppendLine("Veritabanı doğrulama hataları:");

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        var errorMsg = $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}";
                        System.Diagnostics.Debug.WriteLine($"  - {errorMsg}");
                        errorDetails.AppendLine($"• {validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }

                SetErrorMessage($"Veritabanı doğrulama hatası:\n{errorDetails}");
                model.Sectors = GetSectorSelectList();
                return View(model);
            }
            catch (Exception ex)
            {
                // Genel hataları detaylı logla
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] Exception: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Company Create POST] StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Company Create POST] Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[Company Create POST] Inner StackTrace: {ex.InnerException.StackTrace}");
                }

                var errorMessage = $"Şirket oluşturulurken bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nDetay: {ex.InnerException.Message}";
                }

                SetErrorMessage(errorMessage);
                model.Sectors = GetSectorSelectList();
                return View(model);
            }
        }

        // GET: Admin/Company/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = _context.Companies.Find(id);
            if (company == null || company.IsDeleted)
            {
                return HttpNotFound();
            }

            // Debug: Company veritabanından çekilen değerleri logla
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] Company ID: {company.Id}, Code: {company.Code}, Name: {company.Name}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] PaidInCapital: {company.PaidInCapital}, Equity: {company.Equity}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] YearlyNetProfit: {company.YearlyNetProfit}, QuarterlyNetProfit: {company.QuarterlyNetProfit}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] YearlySales: {company.YearlySales}, CurrentMarketValue: {company.CurrentMarketValue}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] PERatio: {company.PERatio}, PBRatio: {company.PBRatio}");

            var model = new CompanyViewModel
            {
                Id = company.Id,
                Code = company.Code,
                Name = company.Name,
                SectorId = company.SectorId,
                Description = company.Description,
                Website = company.Website,
                PaidInCapital = company.PaidInCapital,
                LogoUrl = company.LogoUrl,
                Equity = company.Equity,
                YearlyNetProfit = company.YearlyNetProfit,
                QuarterlyNetProfit = company.QuarterlyNetProfit,
                YearlySales = company.YearlySales,
                CurrentMarketValue = company.CurrentMarketValue,
                PERatio = company.PERatio,
                PBRatio = company.PBRatio,
                NetProfitYear1 = company.NetProfitYear1,
                NetProfitYear2 = company.NetProfitYear2,
                NetProfitYear3 = company.NetProfitYear3,
                NetProfitYear4 = company.NetProfitYear4,
                EquityYear1 = company.EquityYear1,
                EquityYear2 = company.EquityYear2,
                EquityYear3 = company.EquityYear3,
                EquityYear4 = company.EquityYear4,
                PEYear1 = company.PEYear1,
                PEYear2 = company.PEYear2,
                PEYear3 = company.PEYear3,
                LastFinancialUpdate = company.LastFinancialUpdate,
                Sectors = GetSectorSelectList()
            };

            // Debug: ViewModel'e aktarılan değerleri logla
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] ViewModel PaidInCapital: {model.PaidInCapital}, Equity: {model.Equity}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit GET] ViewModel YearlyNetProfit: {model.YearlyNetProfit}, QuarterlyNetProfit: {model.QuarterlyNetProfit}");

            return View(model);
        }

        // POST: Admin/Company/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompanyViewModel model)
        {
            // Debug: Gelen model değerlerini logla
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Model ID: {model.Id}, Code: {model.Code}, Name: {model.Name}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] PaidInCapital: {model.PaidInCapital}, Equity: {model.Equity}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] YearlyNetProfit: {model.YearlyNetProfit}, QuarterlyNetProfit: {model.QuarterlyNetProfit}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] YearlySales: {model.YearlySales}, CurrentMarketValue: {model.CurrentMarketValue}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] PERatio: {model.PERatio}, PBRatio: {model.PBRatio}");
            System.Diagnostics.Debug.WriteLine($"[Company Edit POST] ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                // Model geçersizse, hataları detaylı logla ve kullanıcıya göster
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] ModelState is INVALID. Errors:");
                var errorMessages = new System.Text.StringBuilder();
                errorMessages.AppendLine("Form doğrulama hataları:");

                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        var errorMsg = $"Alan: {modelState.Key}, Hata: {error.ErrorMessage}";
                        System.Diagnostics.Debug.WriteLine($"  - {errorMsg}");
                        errorMessages.AppendLine($"• {modelState.Key}: {error.ErrorMessage}");
                    }
                }

                SetErrorMessage(errorMessages.ToString());
                model.Sectors = GetSectorSelectList();
                return View(model);
            }

            try
            {
                var company = _context.Companies.Find(model.Id);
                if (company == null || company.IsDeleted)
                {
                    System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Company not found or deleted. ID: {model.Id}");
                    SetErrorMessage("Şirket bulunamadı veya silinmiş.");
                    return HttpNotFound();
                }

                // Debug: Güncellemeden önce mevcut değerleri logla
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] BEFORE Update - Company ID: {company.Id}");
                System.Diagnostics.Debug.WriteLine($"  PaidInCapital: {company.PaidInCapital} -> {model.PaidInCapital}");
                System.Diagnostics.Debug.WriteLine($"  Equity: {company.Equity} -> {model.Equity}");
                System.Diagnostics.Debug.WriteLine($"  YearlyNetProfit: {company.YearlyNetProfit} -> {model.YearlyNetProfit}");

                // Temel bilgileri güncelle
                company.Code = model.Code;
                company.Name = model.Name;
                company.SectorId = model.SectorId;
                company.Description = model.Description;
                company.Website = model.Website;
                company.PaidInCapital = model.PaidInCapital;
                company.LogoUrl = model.LogoUrl;

                // Finansal verileri güncelle
                company.Equity = model.Equity;
                company.YearlyNetProfit = model.YearlyNetProfit;
                company.QuarterlyNetProfit = model.QuarterlyNetProfit;
                company.YearlySales = model.YearlySales;
                company.CurrentMarketValue = model.CurrentMarketValue;
                company.PERatio = model.PERatio;
                company.PBRatio = model.PBRatio;

                // Geçmiş yıl verileri
                company.NetProfitYear1 = model.NetProfitYear1;
                company.NetProfitYear2 = model.NetProfitYear2;
                company.NetProfitYear3 = model.NetProfitYear3;
                company.NetProfitYear4 = model.NetProfitYear4;
                company.EquityYear1 = model.EquityYear1;
                company.EquityYear2 = model.EquityYear2;
                company.EquityYear3 = model.EquityYear3;
                company.EquityYear4 = model.EquityYear4;
                company.PEYear1 = model.PEYear1;
                company.PEYear2 = model.PEYear2;
                company.PEYear3 = model.PEYear3;

                company.LastFinancialUpdate = model.LastFinancialUpdate;
                company.UpdatedAt = DateTime.UtcNow;

                // Debug: Entity state'ini kontrol et
                var stateBefore = _context.Entry(company).State;
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Entity State BEFORE marking as Modified: {stateBefore}");

                // Entity Framework'e entity'nin değiştirildiğini açıkça belirt
                _context.Entry(company).State = EntityState.Modified;

                var stateAfter = _context.Entry(company).State;
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Entity State AFTER marking as Modified: {stateAfter}");

                // Debug: Güncellemeden sonra değerleri logla
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] AFTER Assignment:");
                System.Diagnostics.Debug.WriteLine($"  PaidInCapital: {company.PaidInCapital}");
                System.Diagnostics.Debug.WriteLine($"  Equity: {company.Equity}");
                System.Diagnostics.Debug.WriteLine($"  YearlyNetProfit: {company.YearlyNetProfit}");

                // Değişiklikleri veritabanına kaydet
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Calling SaveChanges()...");
                var affectedRows = _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] SaveChanges() completed. Affected rows: {affectedRows}");

                // Log için kaç satır etkilendiğini kontrol et
                if (affectedRows > 0)
                {
                    SetSuccessMessage($"{model.Name} şirketi başarıyla güncellendi. ({affectedRows} kayıt güncellendi)");
                    System.Diagnostics.Debug.WriteLine($"[Company Edit POST] SUCCESS: Updated {affectedRows} rows");
                }
                else
                {
                    SetSuccessMessage($"{model.Name} için değişiklik tespit edilmedi, ancak kayıt tamamlandı.");
                    System.Diagnostics.Debug.WriteLine($"[Company Edit POST] WARNING: SaveChanges returned 0 rows");
                }

                return RedirectToAction("Index");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                // Entity validation hatalarını detaylı logla
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] DbEntityValidationException:");
                var errorDetails = new System.Text.StringBuilder();
                errorDetails.AppendLine("Veritabanı doğrulama hataları:");

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        var errorMsg = $"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}";
                        System.Diagnostics.Debug.WriteLine($"  - {errorMsg}");
                        errorDetails.AppendLine($"• {validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }

                SetErrorMessage($"Veritabanı doğrulama hatası:\n{errorDetails}");
                model.Sectors = GetSectorSelectList();
                return View(model);
            }
            catch (Exception ex)
            {
                // Genel hataları detaylı logla
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Exception: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Company Edit POST] StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"[Company Edit POST] Inner StackTrace: {ex.InnerException.StackTrace}");
                }

                var errorMessage = $"Şirket güncellenirken bir hata oluştu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nDetay: {ex.InnerException.Message}";
                }

                SetErrorMessage(errorMessage);
                model.Sectors = GetSectorSelectList();
                return View(model);
            }
        }

        // GET: Admin/Company/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var company = _context.Companies
                .Include(c => c.Sector)
                .FirstOrDefault(c => c.Id == id);

            if (company == null || company.IsDeleted)
            {
                return HttpNotFound();
            }

            return View(company);
        }

        // POST: Admin/Company/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var company = _context.Companies.Find(id);
            if (company != null)
            {
                company.IsDeleted = true;
                company.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                SetSuccessMessage($"{company.Name} şirketi başarıyla silindi.");
            }

            return RedirectToAction("Index");
        }

        // GET: Admin/Company/FetchFinancialData
        // Not: Bu özellik devre dışı bırakıldı - Türk hisse senetleri için ücretsiz API mevcut değil
        [HttpGet]
        public JsonResult FetchFinancialData(string companyCode)
        {
            return Json(new
            {
                success = false,
                message = "Otomatik finansal veri çekme özelliği şu anda desteklenmemektedir. " +
                          "Türk hisse senetleri için finansal veriler manuel olarak girilmelidir. " +
                          "Finansal verilere KAP (Kamuyu Aydınlatma Platformu) üzerinden ulaşabilirsiniz: https://www.kap.org.tr/tr/bist-sirketler"
            }, JsonRequestBehavior.AllowGet);
        }

        private SelectList GetSectorSelectList(int? selectedValue = null)
        {
            var sectors = _context.Sectors
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToList();

            return new SelectList(sectors, "Id", "Name", selectedValue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
