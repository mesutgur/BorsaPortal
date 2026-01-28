using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class NewsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(500, ErrorMessage = "Başlık en fazla 500 karakter olabilir")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik zorunludur")]
        [Display(Name = "İçerik")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Content { get; set; }

        [StringLength(1000, ErrorMessage = "Özet en fazla 1000 karakter olabilir")]
        [Display(Name = "Özet")]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        [Display(Name = "Yayın Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [StringLength(500, ErrorMessage = "Kaynak en fazla 500 karakter olabilir")]
        [Display(Name = "Kaynak")]
        public string Source { get; set; }

        [StringLength(300, ErrorMessage = "Kaynak URL en fazla 300 karakter olabilir")]
        [Display(Name = "Kaynak URL")]
        [DataType(DataType.Url)]
        public string SourceUrl { get; set; }

        [StringLength(300, ErrorMessage = "Görsel URL en fazla 300 karakter olabilir")]
        [Display(Name = "Görsel URL")]
        [DataType(DataType.Url)]
        public string ImageUrl { get; set; }

        [Display(Name = "Anasayfada Göster")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Şirket")]
        public int? CompanyId { get; set; }

        [Display(Name = "Sektör")]
        public int? SectorId { get; set; }

        [Display(Name = "Görüntülenme")]
        public int ViewCount { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem> Companies { get; set; }
        public IEnumerable<SelectListItem> Sectors { get; set; }
    }
}
