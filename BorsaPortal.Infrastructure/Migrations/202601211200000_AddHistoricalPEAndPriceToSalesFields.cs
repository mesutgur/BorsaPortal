namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddHistoricalPEAndPriceToSalesFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockValuations", "PE_2019", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.StockValuations", "PE_2020", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.StockValuations", "PE_2021", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.StockValuations", "YearlySales", c => c.Decimal(precision: 18, scale: 2));
        }

        public override void Down()
        {
            DropColumn("dbo.StockValuations", "YearlySales");
            DropColumn("dbo.StockValuations", "PE_2021");
            DropColumn("dbo.StockValuations", "PE_2020");
            DropColumn("dbo.StockValuations", "PE_2019");
        }
    }
}
