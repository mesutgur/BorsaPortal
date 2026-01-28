namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddFinancialDataToCompanyAndSector : DbMigration
    {
        public override void Up()
        {
            // Company tablosuna finansal veri alanları ekle
            AddColumn("dbo.Companies", "Equity", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "YearlyNetProfit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "QuarterlyNetProfit", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "YearlySales", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "CurrentMarketValue", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "PERatio", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "PBRatio", c => c.Decimal(precision: 18, scale: 2));

            // Geçmiş yıl verileri
            AddColumn("dbo.Companies", "NetProfitYear1", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "NetProfitYear2", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "NetProfitYear3", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "NetProfitYear4", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "EquityYear1", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "EquityYear2", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "EquityYear3", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "EquityYear4", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "PEYear1", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "PEYear2", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Companies", "PEYear3", c => c.Decimal(precision: 18, scale: 2));

            AddColumn("dbo.Companies", "LastFinancialUpdate", c => c.DateTime());

            // Sector tablosuna sektör ortalamaları ekle
            AddColumn("dbo.Sectors", "AveragePE", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Sectors", "AveragePB", c => c.Decimal(precision: 18, scale: 2));
        }

        public override void Down()
        {
            // Sector alanlarını kaldır
            DropColumn("dbo.Sectors", "AveragePB");
            DropColumn("dbo.Sectors", "AveragePE");

            // Company alanlarını kaldır
            DropColumn("dbo.Companies", "LastFinancialUpdate");
            DropColumn("dbo.Companies", "PEYear3");
            DropColumn("dbo.Companies", "PEYear2");
            DropColumn("dbo.Companies", "PEYear1");
            DropColumn("dbo.Companies", "EquityYear4");
            DropColumn("dbo.Companies", "EquityYear3");
            DropColumn("dbo.Companies", "EquityYear2");
            DropColumn("dbo.Companies", "EquityYear1");
            DropColumn("dbo.Companies", "NetProfitYear4");
            DropColumn("dbo.Companies", "NetProfitYear3");
            DropColumn("dbo.Companies", "NetProfitYear2");
            DropColumn("dbo.Companies", "NetProfitYear1");
            DropColumn("dbo.Companies", "PBRatio");
            DropColumn("dbo.Companies", "PERatio");
            DropColumn("dbo.Companies", "CurrentMarketValue");
            DropColumn("dbo.Companies", "YearlySales");
            DropColumn("dbo.Companies", "QuarterlyNetProfit");
            DropColumn("dbo.Companies", "YearlyNetProfit");
            DropColumn("dbo.Companies", "Equity");
        }
    }
}
