using System.ComponentModel.DataAnnotations;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class MarketIndexViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Endeks kodu zorunludur")]
        [StringLength(20, ErrorMessage = "Endeks kodu en fazla 20 karakter olabilir")]
        [Display(Name = "Endeks Kodu")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Endeks adı zorunludur")]
        [StringLength(100, ErrorMessage = "Endeks adı en fazla 100 karakter olabilir")]
        [Display(Name = "Endeks Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Güncel Değer")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? CurrentValue { get; set; }

        [Display(Name = "Ortalama F/K Oranı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? AveragePE { get; set; }

        [Display(Name = "Ortalama PD/DD Oranı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? AveragePB { get; set; }

        [Display(Name = "Günlük Değişim (%)")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? DailyChange { get; set; }

        [Display(Name = "Toplam Piyasa Değeri")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal? TotalMarketCap { get; set; }
    }
}
