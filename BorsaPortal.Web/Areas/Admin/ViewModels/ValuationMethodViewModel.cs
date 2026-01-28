using System.ComponentModel.DataAnnotations;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class ValuationMethodViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yöntem adı gereklidir")]
        [StringLength(200, ErrorMessage = "Yöntem adı en fazla 200 karakter olabilir")]
        [Display(Name = "Yöntem Adı")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Yöntem tipi gereklidir")]
        [StringLength(50, ErrorMessage = "Yöntem tipi en fazla 50 karakter olabilir")]
        [Display(Name = "Yöntem Tipi")]
        public string MethodType { get; set; }

        [StringLength(2000, ErrorMessage = "Formül en fazla 2000 karakter olabilir")]
        [Display(Name = "Formül")]
        public string Formula { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Sıra")]
        public int DisplayOrder { get; set; }
    }
}
