using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class CompanyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket kodu zorunludur")]
        [StringLength(10, ErrorMessage = "Şirket kodu en fazla 10 karakter olabilir")]
        [Display(Name = "Şirket Kodu")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Şirket adı zorunludur")]
        [StringLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir")]
        [Display(Name = "Şirket Adı")]
        public string Name { get; set; }

        [Display(Name = "Sektör")]
        public int? SectorId { get; set; }

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Website")]
        [StringLength(200)]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string Website { get; set; }

        [Display(Name = "Ödenmiş Sermaye")]
        public decimal? PaidInCapital { get; set; }

        [Display(Name = "Logo URL")]
        [StringLength(300)]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string LogoUrl { get; set; }

        // Financial Data (Son finansal veriler)
        [Display(Name = "Özsermaye")]
        public decimal? Equity { get; set; }

        [Display(Name = "Yıllık Net Kar")]
        public decimal? YearlyNetProfit { get; set; }

        [Display(Name = "Son Çeyrek Net Kar")]
        public decimal? QuarterlyNetProfit { get; set; }

        [Display(Name = "Yıllık Net Satışlar")]
        public decimal? YearlySales { get; set; }

        [Display(Name = "Güncel Piyasa Değeri")]
        public decimal? CurrentMarketValue { get; set; }

        // Calculated Ratios (Hesaplanan oranlar)
        [Display(Name = "F/K Oranı (PER)")]
        public decimal? PERatio { get; set; }

        [Display(Name = "PD/DD Oranı (PBR)")]
        public decimal? PBRatio { get; set; }

        // Historical Financial Data (Geçmiş yıl verileri)
        [Display(Name = "Net Kar (4 Yıl Önce)")]
        public decimal? NetProfitYear1 { get; set; }

        [Display(Name = "Net Kar (3 Yıl Önce)")]
        public decimal? NetProfitYear2 { get; set; }

        [Display(Name = "Net Kar (2 Yıl Önce)")]
        public decimal? NetProfitYear3 { get; set; }

        [Display(Name = "Net Kar (1 Yıl Önce)")]
        public decimal? NetProfitYear4 { get; set; }

        [Display(Name = "Özsermaye (4 Yıl Önce)")]
        public decimal? EquityYear1 { get; set; }

        [Display(Name = "Özsermaye (3 Yıl Önce)")]
        public decimal? EquityYear2 { get; set; }

        [Display(Name = "Özsermaye (2 Yıl Önce)")]
        public decimal? EquityYear3 { get; set; }

        [Display(Name = "Özsermaye (1 Yıl Önce)")]
        public decimal? EquityYear4 { get; set; }

        [Display(Name = "F/K (3 Yıl Önce)")]
        public decimal? PEYear1 { get; set; }

        [Display(Name = "F/K (2 Yıl Önce)")]
        public decimal? PEYear2 { get; set; }

        [Display(Name = "F/K (1 Yıl Önce)")]
        public decimal? PEYear3 { get; set; }

        [Display(Name = "Son Finansal Güncelleme")]
        public DateTime? LastFinancialUpdate { get; set; }

        // For dropdown
        public IEnumerable<SelectListItem> Sectors { get; set; }
    }
}
