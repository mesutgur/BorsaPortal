using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Collections.Generic;

namespace BorsaPortal.Infrastructure.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<BorsaPortalDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;  // Veri kaybını önlemek için false olarak değiştirildi
            ContextKey = "BorsaPortal.Infrastructure.Data.BorsaPortalDbContext";
        }

        protected override void Seed(BorsaPortalDbContext context)
        {
            // Create Admin role if it doesn't exist
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // Create Admin user if it doesn't exist
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var adminEmail = "admin@borsaportal.com";
            var adminUser = userManager.FindByEmail(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = System.DateTime.Now
                };

                var result = userManager.Create(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    userManager.AddToRole(adminUser.Id, "Admin");
                }
            }

            // Seed Sectors (Tüm BIST Sektörleri) - Sadece yoksa ekle
            var sectorsToAdd = new[]
            {
                new { Code = "XU100", Name = "BIST 100", Description = "Borsa İstanbul 100 Endeksi" },
                new { Code = "XBANK", Name = "Bankacılık", Description = "Bankacılık Sektörü" },
                new { Code = "XHOLD", Name = "Holding ve Yatırım", Description = "Holding ve Yatırım Şirketleri" },
                new { Code = "XMANA", Name = "Metal Ana", Description = "Metal Ana Sanayi" },
                new { Code = "XMESY", Name = "Metal Eşya Makine", Description = "Metal Eşya, Makine ve Gereç Yapım" },
                new { Code = "XTAST", Name = "Taş Toprak", Description = "Taş ve Toprağa Dayalı Sanayi" },
                new { Code = "XGIDA", Name = "Gıda İçecek", Description = "Gıda, İçki ve Tütün" },
                new { Code = "XTEKS", Name = "Tekstil Deri", Description = "Tekstil, Deri" },
                new { Code = "XKAGT", Name = "Kağıt", Description = "Orman, Kağıt, Basım" },
                new { Code = "XKMYA", Name = "Kimya Petrol", Description = "Kimya, Petrol, Plastik" },
                new { Code = "XELKT", Name = "Elektrik", Description = "Elektrik, Gaz, Su" },
                new { Code = "XULAS", Name = "Ulaştırma", Description = "Ulaştırma, Depolama, Haberleşme" },
                new { Code = "XTCRT", Name = "Ticaret", Description = "Toptan ve Perakende Ticaret" },
                new { Code = "XBLSM", Name = "Bilişim", Description = "Bilişim ve Teknoloji" },
                new { Code = "XSGRT", Name = "Sigorta", Description = "Sigorta Şirketleri" },
                new { Code = "XFINK", Name = "Finansal Kiralama", Description = "Finansal Kiralama ve Faktoring" },
                new { Code = "XGMYO", Name = "GYO", Description = "Gayrimenkul Yatırım Ortaklığı" },
                new { Code = "XUHIZ", Name = "Hizmet", Description = "Hizmet Sektörü" },
                new { Code = "XSPOR", Name = "Spor", Description = "Spor Kulüpleri" },
                new { Code = "XILTM", Name = "İletişim", Description = "İletişim Teknolojileri" }
            };

            foreach (var sectorData in sectorsToAdd)
            {
                if (!context.Sectors.Any(s => s.Code == sectorData.Code))
                {
                    context.Sectors.Add(new Sector
                    {
                        Code = sectorData.Code,
                        Name = sectorData.Name,
                        Description = sectorData.Description,
                        IsDeleted = false,
                        CreatedAt = System.DateTime.Now
                    });
                }
            }

            context.SaveChanges();

            // Get sector IDs
            var bist100 = context.Sectors.FirstOrDefault(s => s.Code == "XU100");
            var banking = context.Sectors.FirstOrDefault(s => s.Code == "XBANK");
            var holding = context.Sectors.FirstOrDefault(s => s.Code == "XHOLD");
            var metalMain = context.Sectors.FirstOrDefault(s => s.Code == "XMANA");
            var metalGoods = context.Sectors.FirstOrDefault(s => s.Code == "XMESY");
            var stoneEarth = context.Sectors.FirstOrDefault(s => s.Code == "XTAST");
            var food = context.Sectors.FirstOrDefault(s => s.Code == "XGIDA");
            var textile = context.Sectors.FirstOrDefault(s => s.Code == "XTEKS");
            var paper = context.Sectors.FirstOrDefault(s => s.Code == "XKAGT");
            var chemistry = context.Sectors.FirstOrDefault(s => s.Code == "XKMYA");
            var electric = context.Sectors.FirstOrDefault(s => s.Code == "XELKT");
            var transport = context.Sectors.FirstOrDefault(s => s.Code == "XULAS");
            var trade = context.Sectors.FirstOrDefault(s => s.Code == "XTCRT");
            var tech = context.Sectors.FirstOrDefault(s => s.Code == "XBLSM");
            var insurance = context.Sectors.FirstOrDefault(s => s.Code == "XSGRT");
            var leasing = context.Sectors.FirstOrDefault(s => s.Code == "XFINK");
            var reit = context.Sectors.FirstOrDefault(s => s.Code == "XGMYO");
            var service = context.Sectors.FirstOrDefault(s => s.Code == "XUHIZ");
            var sports = context.Sectors.FirstOrDefault(s => s.Code == "XSPOR");
            var telecom = context.Sectors.FirstOrDefault(s => s.Code == "XILTM");

            // Seed Companies (BIST'teki Önemli Şirketler) - Sadece yoksa ekle
            var companiesToAdd = new List<Company>
            {
                // Bankacılık (XBANK)
                new Company { Code = "AKBNK", Name = "Akbank", SectorId = banking?.Id },
                new Company { Code = "GARAN", Name = "Garanti BBVA", SectorId = banking?.Id },
                new Company { Code = "ISCTR", Name = "İş Bankası (C)", SectorId = banking?.Id },
                new Company { Code = "YKBNK", Name = "Yapı Kredi Bankası", SectorId = banking?.Id },
                new Company { Code = "HALKB", Name = "Halkbank", SectorId = banking?.Id },
                new Company { Code = "VAKBN", Name = "Vakıfbank", SectorId = banking?.Id },
                new Company { Code = "QNBFB", Name = "QNB Finansbank", SectorId = banking?.Id },
                new Company { Code = "ALBRK", Name = "Albaraka Türk", SectorId = banking?.Id },
                new Company { Code = "TSKB", Name = "Türkiye Sınai Kalkınma Bankası", SectorId = banking?.Id },
                new Company { Code = "SKBNK", Name = "Şekerbank", SectorId = banking?.Id },
                new Company { Code = "ICBCT", Name = "ICBC Turkey Bank", SectorId = banking?.Id },
                new Company { Code = "DENIZ", Name = "Denizbank", SectorId = banking?.Id },

                // Holding ve Yatırım (XHOLD)
                new Company { Code = "SAHOL", Name = "Sabancı Holding", SectorId = holding?.Id },
                new Company { Code = "KCHOL", Name = "Koç Holding", SectorId = holding?.Id },
                new Company { Code = "DOHOL", Name = "Doğan Holding", SectorId = holding?.Id },
                new Company { Code = "AGHOL", Name = "Anadolu Grubu Holding", SectorId = holding?.Id },
                new Company { Code = "GOZDE", Name = "Gözde Girişim", SectorId = holding?.Id },
                new Company { Code = "ECZYT", Name = "Eczacıbaşı Yatırım", SectorId = holding?.Id },
                new Company { Code = "THYAO", Name = "Türk Hava Yolları", SectorId = holding?.Id },
                new Company { Code = "TAVHL", Name = "TAV Havalimanları", SectorId = holding?.Id },

                // Metal Ana (XMANA)
                new Company { Code = "EREGL", Name = "Ereğli Demir Çelik", SectorId = metalMain?.Id },
                new Company { Code = "KRDMD", Name = "Kardemir (D)", SectorId = metalMain?.Id },
                new Company { Code = "IZMDC", Name = "İzmir Demir Çelik", SectorId = metalMain?.Id },
                new Company { Code = "ERBOS", Name = "Erbosan", SectorId = metalMain?.Id },
                new Company { Code = "CEMTS", Name = "Cemtaş", SectorId = metalMain?.Id },

                // Metal Eşya, Makine (XMESY)
                new Company { Code = "ASELS", Name = "Aselsan", SectorId = metalGoods?.Id },
                new Company { Code = "ARCLK", Name = "Arçelik", SectorId = metalGoods?.Id },
                new Company { Code = "VESTL", Name = "Vestel", SectorId = metalGoods?.Id },
                new Company { Code = "TOASO", Name = "Tofaş", SectorId = metalGoods?.Id },
                new Company { Code = "FROTO", Name = "Ford Otosan", SectorId = metalGoods?.Id },
                new Company { Code = "OTKAR", Name = "Otokar", SectorId = metalGoods?.Id },
                new Company { Code = "BFREN", Name = "Bosch Fren Sistemleri", SectorId = metalGoods?.Id },
                new Company { Code = "EMKEL", Name = "Emek Elektrik", SectorId = metalGoods?.Id },

                // Taş Toprak (XTAST)
                new Company { Code = "SISE", Name = "Şişe Cam", SectorId = stoneEarth?.Id },
                new Company { Code = "CIMSA", Name = "Çimsa", SectorId = stoneEarth?.Id },
                new Company { Code = "AKCNS", Name = "Akçansa", SectorId = stoneEarth?.Id },
                new Company { Code = "BTCIM", Name = "Batıçim", SectorId = stoneEarth?.Id },
                new Company { Code = "GOLTS", Name = "Göltaş", SectorId = stoneEarth?.Id },
                new Company { Code = "KONYA", Name = "Konya Çimento", SectorId = stoneEarth?.Id },

                // Gıda İçecek (XGIDA)
                new Company { Code = "BIMAS", Name = "BIM", SectorId = food?.Id },
                new Company { Code = "MGROS", Name = "Migros", SectorId = food?.Id },
                new Company { Code = "SOKM", Name = "Şok Marketler", SectorId = food?.Id },
                new Company { Code = "ULKER", Name = "Ülker", SectorId = food?.Id },
                new Company { Code = "CCOLA", Name = "Coca Cola İçecek", SectorId = food?.Id },
                new Company { Code = "AEFES", Name = "Anadolu Efes", SectorId = food?.Id },
                new Company { Code = "TATGD", Name = "Tat Gıda", SectorId = food?.Id },
                new Company { Code = "BANVT", Name = "Banvit", SectorId = food?.Id },
                new Company { Code = "KNFRT", Name = "Konfrut", SectorId = food?.Id },
                new Company { Code = "PENGD", Name = "Penguen Gıda", SectorId = food?.Id },

                // Tekstil Deri (XTEKS)
                new Company { Code = "BRISA", Name = "Brisa", SectorId = textile?.Id },
                new Company { Code = "KORDS", Name = "Kordsa", SectorId = textile?.Id },
                new Company { Code = "YUNSA", Name = "Yünsa", SectorId = textile?.Id },
                new Company { Code = "BOSSA", Name = "Bossa", SectorId = textile?.Id },
                new Company { Code = "DESA", Name = "Desa Deri", SectorId = textile?.Id },
                new Company { Code = "BLCYT", Name = "Bilici Yatırım", SectorId = textile?.Id },

                // Kağıt Basım (XKAGT)
                new Company { Code = "PRKME", Name = "Park Elektrik Madencilik", SectorId = paper?.Id },
                new Company { Code = "KARTN", Name = "Kartonsan", SectorId = paper?.Id },
                new Company { Code = "OLMKS", Name = "Olmuksan", SectorId = paper?.Id },

                // Kimya Petrol (XKMYA)
                new Company { Code = "TUPRS", Name = "Tüpraş", SectorId = chemistry?.Id },
                new Company { Code = "PETKM", Name = "Petkim", SectorId = chemistry?.Id },
                new Company { Code = "SODA", Name = "Soda Sanayii", SectorId = chemistry?.Id },
                new Company { Code = "GUBRF", Name = "Gübre Fabrikaları", SectorId = chemistry?.Id },
                new Company { Code = "AYGAZ", Name = "Aygaz", SectorId = chemistry?.Id },
                new Company { Code = "AKSA", Name = "Aksa", SectorId = chemistry?.Id },
                new Company { Code = "BRKSN", Name = "Berkosan", SectorId = chemistry?.Id },
                new Company { Code = "DYOBY", Name = "Dyo Boya", SectorId = chemistry?.Id },

                // Elektrik Gaz Su (XELKT)
                new Company { Code = "AKSEN", Name = "Aksa Enerji", SectorId = electric?.Id },
                new Company { Code = "AKENR", Name = "Akenerji", SectorId = electric?.Id },
                new Company { Code = "ENKA", Name = "Enka İnşaat", SectorId = electric?.Id },
                new Company { Code = "ZOREN", Name = "Zorlu Enerji", SectorId = electric?.Id },
                new Company { Code = "GENIL", Name = "Gen İlaç", SectorId = electric?.Id },
                new Company { Code = "HUNER", Name = "Hun Yenilenebilir Enerji", SectorId = electric?.Id },

                // Ulaştırma (XULAS)
                new Company { Code = "PGSUS", Name = "Pegasus", SectorId = transport?.Id },
                new Company { Code = "CLEBI", Name = "Çelebi Hava Servisi", SectorId = transport?.Id },
                new Company { Code = "GSDHO", Name = "GSD Holding", SectorId = transport?.Id },
                new Company { Code = "RYSAS", Name = "Reysaş", SectorId = transport?.Id },

                // Ticaret (XTCRT)
                new Company { Code = "CRFSA", Name = "Carrefoursa", SectorId = trade?.Id },
                new Company { Code = "MAVI", Name = "Mavi Giyim", SectorId = trade?.Id },
                new Company { Code = "MEPET", Name = "Mepet", SectorId = trade?.Id },
                new Company { Code = "BIZIM", Name = "Bizim Toptan", SectorId = trade?.Id },

                // Teknoloji (XBLSM)
                new Company { Code = "LOGO", Name = "Logo Yazılım", SectorId = tech?.Id },
                new Company { Code = "NETAS", Name = "Netaş", SectorId = tech?.Id },
                new Company { Code = "ALCTL", Name = "Alcatel Lucent Teletaş", SectorId = tech?.Id },
                new Company { Code = "INDES", Name = "İndeks Bilgisayar", SectorId = tech?.Id },
                new Company { Code = "LINK", Name = "Link Bilgisayar", SectorId = tech?.Id },
                new Company { Code = "ESCOM", Name = "Escort Teknoloji", SectorId = tech?.Id },

                // İletişim (XILTM)
                new Company { Code = "TTKOM", Name = "Türk Telekom", SectorId = telecom?.Id },
                new Company { Code = "TCELL", Name = "Turkcell", SectorId = telecom?.Id },
                new Company { Code = "ARENA", Name = "Arena Bilgisayar", SectorId = telecom?.Id },

                // Sigorta (XSGRT)
                new Company { Code = "AGESA", Name = "Anadolu Hayat Emeklilik", SectorId = insurance?.Id },
                new Company { Code = "AKGRT", Name = "Aksigorta", SectorId = insurance?.Id },
                new Company { Code = "ANHYT", Name = "Anadolu Hayat", SectorId = insurance?.Id },
                new Company { Code = "GUSGR", Name = "Güneş Sigorta", SectorId = insurance?.Id },
                new Company { Code = "RAYSG", Name = "Ray Sigorta", SectorId = insurance?.Id },

                // Finansal Kiralama (XFINK)
                new Company { Code = "ALFAS", Name = "Alfa Solar Enerji", SectorId = leasing?.Id },
                new Company { Code = "SMRFA", Name = "Smart Faktoring", SectorId = leasing?.Id },

                // GYO (XGMYO)
                new Company { Code = "EKGYO", Name = "Emlak Konut GYO", SectorId = reit?.Id },
                new Company { Code = "ISGYO", Name = "İş GYO", SectorId = reit?.Id },
                new Company { Code = "TRGYO", Name = "Torunlar GYO", SectorId = reit?.Id },
                new Company { Code = "AKFGY", Name = "Akfen GYO", SectorId = reit?.Id },
                new Company { Code = "AVGYO", Name = "Avrasya GYO", SectorId = reit?.Id },
                new Company { Code = "DGGYO", Name = "Doğuş GYO", SectorId = reit?.Id },

                // Hizmet (XUHIZ)
                new Company { Code = "MPARK", Name = "MLP Sağlık", SectorId = service?.Id },
                new Company { Code = "OZKGY", Name = "Özak GYO", SectorId = service?.Id },
                new Company { Code = "VERTU", Name = "Verusaturk Girişim", SectorId = service?.Id },

                // Spor (XSPOR)
                new Company { Code = "BJKAS", Name = "Beşiktaş", SectorId = sports?.Id },
                new Company { Code = "FENER", Name = "Fenerbahçe", SectorId = sports?.Id },
                new Company { Code = "GSRAY", Name = "Galatasaray", SectorId = sports?.Id },
                new Company { Code = "TSPOR", Name = "Trabzonspor", SectorId = sports?.Id }
            };

            foreach (var company in companiesToAdd)
            {
                if (!context.Companies.Any(c => c.Code == company.Code))
                {
                    company.IsDeleted = false;
                    company.CreatedAt = System.DateTime.Now;
                    context.Companies.Add(company);
                }
            }

            context.SaveChanges();

            // Seed Market Indices (BIST100, etc.) - Sadece yoksa ekle
            var marketIndicesToAdd = new[]
            {
                new { Code = "XU100", Name = "BIST 100", Description = "Borsa İstanbul 100 Endeksi - En yüksek piyasa değerine sahip 100 şirket", AveragePE = 12.5m, AveragePB = 2.1m },
                new { Code = "XU030", Name = "BIST 30", Description = "Borsa İstanbul 30 Endeksi - En yüksek piyasa değerine sahip 30 şirket", AveragePE = 11.8m, AveragePB = 1.9m },
                new { Code = "XUSIN", Name = "BIST Sınai", Description = "Borsa İstanbul Sınai Endeksi", AveragePE = 13.2m, AveragePB = 2.3m },
                new { Code = "XUHIZ", Name = "BIST Hizmetler", Description = "Borsa İstanbul Hizmetler Endeksi", AveragePE = 14.5m, AveragePB = 2.5m },
                new { Code = "XUMAL", Name = "BIST Mali", Description = "Borsa İstanbul Mali Endeksi", AveragePE = 10.2m, AveragePB = 1.7m }
            };

            foreach (var indexData in marketIndicesToAdd)
            {
                var existingIndex = context.MarketIndices.FirstOrDefault(m => m.Code == indexData.Code);
                if (existingIndex == null)
                {
                    context.MarketIndices.Add(new MarketIndex
                    {
                        Code = indexData.Code,
                        Name = indexData.Name,
                        Description = indexData.Description,
                        AveragePE = indexData.AveragePE,
                        AveragePB = indexData.AveragePB,
                        IsDeleted = false,
                        CreatedAt = System.DateTime.Now
                    });
                }
                else
                {
                    // Var olan kayıt varsa, AveragePE ve AveragePB değerlerini güncelle
                    if (!existingIndex.AveragePE.HasValue || !existingIndex.AveragePB.HasValue)
                    {
                        existingIndex.AveragePE = indexData.AveragePE;
                        existingIndex.AveragePB = indexData.AveragePB;
                    }
                }
            }

            context.SaveChanges();

            // Seed Valuation Methods - Sadece yoksa ekle
            var valuationMethodsToAdd = new[]
            {
                new { MethodType = "Basic", Name = "Temel Değerleme", Description = "HBK × F/K yöntemi ile temel değerleme", Formula = "HBK = Yıllık Net Kar / Ödenmiş Sermaye; Hisse Fiyat = HBK × F/K Oranı", DisplayOrder = 1 },
                new { MethodType = "Quarter1", Name = "1. Çeyrek Değerleme", Description = "3 aylık finansal verilere göre değerleme", Formula = "6 farklı yöntem: Cari F/K, Future's F/K, PD/DD, Ödenmiş Sermaye, Potansiyel Piyasa Değeri, Özsermaye Karlılığı", DisplayOrder = 2 },
                new { MethodType = "Quarter2", Name = "2. Çeyrek Değerleme", Description = "6 aylık finansal verilere göre değerleme", Formula = "6 farklı yöntem: Cari F/K, Future's F/K, PD/DD, Ödenmiş Sermaye, Potansiyel Piyasa Değeri, Özsermaye Karlılığı", DisplayOrder = 3 },
                new { MethodType = "Quarter3", Name = "3. Çeyrek Değerleme", Description = "9 aylık finansal verilere göre değerleme", Formula = "6 farklı yöntem: Cari F/K, Future's F/K, PD/DD, Ödenmiş Sermaye, Potansiyel Piyasa Değeri, Özsermaye Karlılığı", DisplayOrder = 4 },
                new { MethodType = "Quarter4", Name = "4. Çeyrek (Yıllık) Değerleme", Description = "Yıllık finansal verilere göre değerleme", Formula = "5 farklı yöntem: Cari F/K, PD/DD, Ödenmiş Sermaye, Potansiyel Piyasa Değeri, Özsermaye Karlılığı", DisplayOrder = 5 },
                new { MethodType = "YearEnd", Name = "Yıl Sonu Tahmini", Description = "Yıl sonu tahminlerine göre değerleme", Formula = "Future's F/K yöntemi ile tahmini değerleme", DisplayOrder = 6 },
                new { MethodType = "LongTerm", Name = "Uzun Vade Hedef", Description = "Geçmiş performans bazlı uzun vade değerleme", Formula = "Geçmiş yıl kar ve özsermaye artış oranlarına göre tahmin", DisplayOrder = 7 },
                new { MethodType = "HistoricalPE", Name = "Tarihsel F/K Değerlemesi", Description = "Son 3 yılın ortalama F/K oranına göre değerleme", Formula = "(Hisse Fiyatı / Şuan ki F/K) × Tarihsel F/K Oranı", DisplayOrder = 8 },
                new { MethodType = "PriceToSales", Name = "PD/NS Yöntemi", Description = "Piyasa Değeri / Net Satış oranına göre değerleme", Formula = "Net Kar Marjı = (Yıllık Net Kar / Yıllık Net Satış) × 10; PD/NS = Piyasa Değeri / Yıllık Net Satış; Değer = (Net Kar Marjı / PD/NS) × Hisse Fiyatı", DisplayOrder = 9 }
            };

            foreach (var methodData in valuationMethodsToAdd)
            {
                if (!context.ValuationMethods.Any(vm => vm.MethodType == methodData.MethodType))
                {
                    context.ValuationMethods.Add(new ValuationMethod
                    {
                        MethodType = methodData.MethodType,
                        Name = methodData.Name,
                        Description = methodData.Description,
                        Formula = methodData.Formula,
                        IsActive = true,
                        DisplayOrder = methodData.DisplayOrder
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
