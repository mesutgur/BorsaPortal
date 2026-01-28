using System;
using System.ComponentModel.DataAnnotations;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class RssFeedSourceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kaynak adı gereklidir")]
        [Display(Name = "Kaynak Adı")]
        [StringLength(200, ErrorMessage = "Kaynak adı en fazla 200 karakter olabilir")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "RSS Feed URL'i gereklidir")]
        [Display(Name = "RSS Feed URL")]
        [StringLength(1000, ErrorMessage = "URL en fazla 1000 karakter olabilir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string FeedUrl { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Son Çekilme Zamanı")]
        public DateTime? LastFetchedAt { get; set; }

        [Required(ErrorMessage = "Çekilme aralığı gereklidir")]
        [Display(Name = "Çekilme Aralığı (Dakika)")]
        [Range(15, 1440, ErrorMessage = "Çekilme aralığı 15-1440 dakika arasında olmalıdır")]
        public int FetchIntervalMinutes { get; set; }

        [Display(Name = "Sektör")]
        public int? SectorId { get; set; }

        [Display(Name = "Toplam Çekilen Haber")]
        public int ItemsFetchedCount { get; set; }

        [Display(Name = "İçermesi Gereken Anahtar Kelimeler")]
        [StringLength(500, ErrorMessage = "Anahtar kelimeler en fazla 500 karakter olabilir")]
        public string IncludeKeywords { get; set; }

        [Display(Name = "İçermemesi Gereken Anahtar Kelimeler")]
        [StringLength(500, ErrorMessage = "Anahtar kelimeler en fazla 500 karakter olabilir")]
        public string ExcludeKeywords { get; set; }

        [Display(Name = "Otomatik Yayınla")]
        public bool AutoPublish { get; set; }
    }
}
