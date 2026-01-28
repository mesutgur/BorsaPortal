using BorsaPortal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BorsaPortal.Web.Areas.Admin.ViewModels
{
    public class KapNotificationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Şirket seçilmelidir")]
        [Display(Name = "Şirket")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur")]
        [StringLength(500)]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Display(Name = "Özet")]
        [DataType(DataType.MultilineText)]
        public string Summary { get; set; }

        [Display(Name = "İçerik")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Content { get; set; }

        [Required]
        [Display(Name = "Yayın Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [Required]
        [Display(Name = "Bildirim Türü")]
        public KapNotificationType NotificationType { get; set; }

        [StringLength(50)]
        [Display(Name = "Bildirim Numarası")]
        public string NotificationNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Kaynak URL")]
        [Url]
        public string SourceUrl { get; set; }

        [Display(Name = "Otomatik Çekildi")]
        public bool IsAutoFetched { get; set; }

        // For display
        public string CompanyName { get; set; }

        // For dropdowns
        public IEnumerable<SelectListItem> Companies { get; set; }
        public IEnumerable<SelectListItem> NotificationTypes { get; set; }
    }
}
