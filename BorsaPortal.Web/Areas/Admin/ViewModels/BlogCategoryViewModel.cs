using System.ComponentModel.DataAnnotations;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class BlogCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Slug zorunludur")]
        [StringLength(150, ErrorMessage = "Slug en fazla 150 karakter olabilir")]
        [Display(Name = "Slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug sadece küçük harf, rakam ve tire içerebilir")]
        public string Slug { get; set; }

        [Display(Name = "Açıklama")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Sıralama")]
        [Range(0, 1000, ErrorMessage = "Sıralama 0-1000 arasında olmalıdır")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Yazı Sayısı")]
        public int PostCount { get; set; }
    }
}
