using BorsaPortal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class BlogPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(300, ErrorMessage = "Başlık en fazla 300 karakter olabilir")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Slug zorunludur")]
        [StringLength(350, ErrorMessage = "Slug en fazla 350 karakter olabilir")]
        [Display(Name = "Slug")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug sadece küçük harf, rakam ve tire içerebilir")]
        public string Slug { get; set; }

        [Required(ErrorMessage = "İçerik zorunludur")]
        [Display(Name = "İçerik")]
        [DataType(DataType.Html)]
        [AllowHtml]
        public string Body { get; set; }

        [StringLength(500)]
        [Display(Name = "Özet")]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        [Display(Name = "Öne Çıkan Görsel URL")]
        [StringLength(300)]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string FeaturedImageUrl { get; set; }

        [StringLength(150)]
        [Display(Name = "SEO Başlık")]
        public string SeoTitle { get; set; }

        [StringLength(300)]
        [Display(Name = "SEO Açıklama")]
        [DataType(DataType.MultilineText)]
        public string SeoDescription { get; set; }

        [StringLength(200)]
        [Display(Name = "SEO Anahtar Kelimeler")]
        public string SeoKeywords { get; set; }

        [Required]
        [Display(Name = "Durum")]
        public PublishStatus Status { get; set; }

        [Display(Name = "Yayın Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime? PublishedAt { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Etiketler")]
        public string TagsInput { get; set; } // Comma-separated tags

        [Display(Name = "Görüntülenme")]
        public int ViewCount { get; set; }

        [Display(Name = "Yorum Sayısı")]
        public int CommentCount { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
    }
}
