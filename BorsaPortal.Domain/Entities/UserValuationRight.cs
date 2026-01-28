using System;

namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcının değerleme yapma hakkını yönetir
    /// </summary>
    public class UserValuationRight : BaseEntity
    {
        public string UserId { get; set; }
        public int RemainingValuations { get; set; } // Kalan değerleme hakkı
        public int TotalValuations { get; set; } // Toplam kullanılabilir değerleme hakkı
        public DateTime? ExpiryDate { get; set; } // Hakkın geçerlilik tarihi
        public bool IsUnlimited { get; set; } // Sınırsız hak var mı?

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }

        public UserValuationRight()
        {
            IsUnlimited = false;
            RemainingValuations = 0;
            TotalValuations = 0;
        }

        public bool CanMakeValuation()
        {
            if (IsUnlimited)
                return true;

            if (ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow)
                return false;

            return RemainingValuations > 0;
        }

        public void UseValuation()
        {
            if (!IsUnlimited && RemainingValuations > 0)
            {
                RemainingValuations--;
            }
        }
    }
}
