namespace BorsaPortal.Infrastructure.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MakeValuationMethodIdNullable : DbMigration
    {
        public override void Up()
        {
            // ValuationMethodId sütununu nullable yap
            AlterColumn("dbo.StockValuations", "ValuationMethodId", c => c.Int());
        }

        public override void Down()
        {
            // Geri alma: nullable'dan required'a döndür
            AlterColumn("dbo.StockValuations", "ValuationMethodId", c => c.Int(nullable: false));
        }
    }
}
