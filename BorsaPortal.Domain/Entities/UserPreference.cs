namespace BorsaPortal.Domain.Entities
{
    /// <summary>
    /// Kullanıcı tercihleri ve ayarları
    /// </summary>
    public class UserPreference : BaseEntity
    {
        public string UserId { get; set; }

        // Notification Settings
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool NotifyOnKapDisclosures { get; set; }
        public bool NotifyOnPriceChanges { get; set; }
        public bool NotifyOnNewsPublished { get; set; }

        // Display Settings
        public string PreferredLanguage { get; set; }
        public string PreferredCurrency { get; set; }
        public int ItemsPerPage { get; set; }

        // Alert Settings
        public decimal? PriceChangeAlertPercentage { get; set; }
        public bool DailyDigestEnabled { get; set; }
        public string DailyDigestTime { get; set; } // HH:mm format

        // Privacy Settings
        public bool ProfileVisible { get; set; }
        public bool PortfolioVisible { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; }

        public UserPreference()
        {
            EmailNotificationsEnabled = true;
            NotifyOnKapDisclosures = true;
            PreferredLanguage = "tr-TR";
            PreferredCurrency = "TRY";
            ItemsPerPage = 20;
            ProfileVisible = false;
            PortfolioVisible = false;
        }
    }
}
