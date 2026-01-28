namespace BorsaPortal.Domain.Entities
{
    public class Sector : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }

        // Sector Averages (Sektör Ortalamaları)
        public decimal? AveragePE { get; set; } // Sektör Ortalama F/K Oranı
        public decimal? AveragePB { get; set; } // Sektör Ortalama PD/DD Oranı
    }
}
