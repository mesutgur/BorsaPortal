namespace BorsaPortal.Infrastructure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RenameYearColumnsToGenericNames : DbMigration
    {
        public override void Up()
        {
            // Uzun vade için Net Kar sütunlarını yeniden adlandır
            RenameColumn(table: "dbo.StockValuations", name: "NetProfit2019", newName: "NetProfitYear1");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfit2020", newName: "NetProfitYear2");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfit2021", newName: "NetProfitYear3");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfit2022", newName: "NetProfitYear4");

            // Uzun vade için Özsermaye sütunlarını yeniden adlandır
            RenameColumn(table: "dbo.StockValuations", name: "Equity2019", newName: "EquityYear1");
            RenameColumn(table: "dbo.StockValuations", name: "Equity2020", newName: "EquityYear2");
            RenameColumn(table: "dbo.StockValuations", name: "Equity2021", newName: "EquityYear3");
            RenameColumn(table: "dbo.StockValuations", name: "Equity2022", newName: "EquityYear4");

            // Tarihsel F/K için sütunları yeniden adlandır
            RenameColumn(table: "dbo.StockValuations", name: "PE_2019", newName: "PE_Year1");
            RenameColumn(table: "dbo.StockValuations", name: "PE_2020", newName: "PE_Year2");
            RenameColumn(table: "dbo.StockValuations", name: "PE_2021", newName: "PE_Year3");
        }

        public override void Down()
        {
            // Geri alma işlemleri
            RenameColumn(table: "dbo.StockValuations", name: "NetProfitYear1", newName: "NetProfit2019");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfitYear2", newName: "NetProfit2020");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfitYear3", newName: "NetProfit2021");
            RenameColumn(table: "dbo.StockValuations", name: "NetProfitYear4", newName: "NetProfit2022");

            RenameColumn(table: "dbo.StockValuations", name: "EquityYear1", newName: "Equity2019");
            RenameColumn(table: "dbo.StockValuations", name: "EquityYear2", newName: "Equity2020");
            RenameColumn(table: "dbo.StockValuations", name: "EquityYear3", newName: "Equity2021");
            RenameColumn(table: "dbo.StockValuations", name: "EquityYear4", newName: "Equity2022");

            RenameColumn(table: "dbo.StockValuations", name: "PE_Year1", newName: "PE_2019");
            RenameColumn(table: "dbo.StockValuations", name: "PE_Year2", newName: "PE_2020");
            RenameColumn(table: "dbo.StockValuations", name: "PE_Year3", newName: "PE_2021");
        }
    }
}
