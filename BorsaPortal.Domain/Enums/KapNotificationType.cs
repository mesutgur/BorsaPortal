namespace BorsaPortal.Domain.Enums
{
    public enum KapNotificationType
    {
        FinancialStatement = 1,      // Mali Tablo
        GeneralAssembly = 2,         // Genel Kurul
        OwnershipStructure = 3,      // Ortaklık Yapısı
        MaterialDisclosure = 4,      // Özel Durum Açıklaması
        Dividend = 5,                // Kar Payı
        BoardDecision = 6,           // Yönetim Kurulu Kararı
        CapitalIncrease = 7,         // Sermaye Artırımı
        MergerAcquisition = 8,       // Birleşme/Devralma
        Related = 9,                 // İlişkili Taraf İşlemleri
        Other = 99                   // Diğer
    }
}
