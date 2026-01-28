using System.ComponentModel.DataAnnotations;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class SectorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sektör adı zorunludur")]
        [StringLength(100, ErrorMessage = "Sektör adı en fazla 100 karakter olabilir")]
        [Display(Name = "Sektör Adı")]
        public string Name { get; set; }

        [StringLength(20, ErrorMessage = "Sektör kodu en fazla 20 karakter olabilir")]
        [Display(Name = "Sektör Kodu")]
        public string Code { get; set; }

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Ortalama F/K Oranı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? AveragePE { get; set; }

        [Display(Name = "Ortalama PD/DD Oranı")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? AveragePB { get; set; }
    }
}
