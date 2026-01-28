namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddValuationEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockValuations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CompanyId = c.Int(nullable: false),
                        ValuationMethodId = c.Int(nullable: false),
                        CurrentPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaidInCapital = c.Decimal(precision: 18, scale: 2),
                        QuarterlyNetProfit = c.Decimal(precision: 18, scale: 2),
                        SixMonthNetProfit = c.Decimal(precision: 18, scale: 2),
                        NineMonthNetProfit = c.Decimal(precision: 18, scale: 2),
                        YearlyNetProfit = c.Decimal(precision: 18, scale: 2),
                        Equity = c.Decimal(precision: 18, scale: 2),
                        CurrentMarketValue = c.Decimal(precision: 18, scale: 2),
                        StockPE = c.Decimal(precision: 18, scale: 2),
                        StockPB = c.Decimal(precision: 18, scale: 2),
                        SectorPE = c.Decimal(precision: 18, scale: 2),
                        SectorPB = c.Decimal(precision: 18, scale: 2),
                        NetProfit2019 = c.Decimal(precision: 18, scale: 2),
                        NetProfit2020 = c.Decimal(precision: 18, scale: 2),
                        NetProfit2021 = c.Decimal(precision: 18, scale: 2),
                        NetProfit2022 = c.Decimal(precision: 18, scale: 2),
                        Equity2019 = c.Decimal(precision: 18, scale: 2),
                        Equity2020 = c.Decimal(precision: 18, scale: 2),
                        Equity2021 = c.Decimal(precision: 18, scale: 2),
                        Equity2022 = c.Decimal(precision: 18, scale: 2),
                        EstimatedYearEndProfit = c.Decimal(precision: 18, scale: 2),
                        TargetPrice = c.Decimal(precision: 18, scale: 2),
                        PremiumPotential = c.Decimal(precision: 18, scale: 2),
                        Notes = c.String(maxLength: 2000),
                        ValuationDate = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.ValuationMethods", t => t.ValuationMethodId)
                .Index(t => t.UserId)
                .Index(t => t.CompanyId)
                .Index(t => t.ValuationMethodId);
            
            CreateTable(
                "dbo.ValuationCalculations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StockValuationId = c.Int(nullable: false),
                        CalculationType = c.String(nullable: false, maxLength: 50),
                        CalculationName = c.String(nullable: false, maxLength: 200),
                        CalculatedPrice = c.Decimal(precision: 18, scale: 2),
                        Formula = c.String(maxLength: 1000),
                        FormulaWithValues = c.String(maxLength: 1000),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StockValuations", t => t.StockValuationId, cascadeDelete: true)
                .Index(t => t.StockValuationId);
            
            CreateTable(
                "dbo.ValuationMethods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 1000),
                        MethodType = c.String(nullable: false, maxLength: 50),
                        Formula = c.String(maxLength: 2000),
                        IsActive = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserValuationRights",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        RemainingValuations = c.Int(nullable: false),
                        TotalValuations = c.Int(nullable: false),
                        ExpiryDate = c.DateTime(),
                        IsUnlimited = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserValuationRights", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StockValuations", "ValuationMethodId", "dbo.ValuationMethods");
            DropForeignKey("dbo.StockValuations", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.StockValuations", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.ValuationCalculations", "StockValuationId", "dbo.StockValuations");
            DropIndex("dbo.UserValuationRights", new[] { "UserId" });
            DropIndex("dbo.ValuationCalculations", new[] { "StockValuationId" });
            DropIndex("dbo.StockValuations", new[] { "ValuationMethodId" });
            DropIndex("dbo.StockValuations", new[] { "CompanyId" });
            DropIndex("dbo.StockValuations", new[] { "UserId" });
            DropTable("dbo.UserValuationRights");
            DropTable("dbo.ValuationMethods");
            DropTable("dbo.ValuationCalculations");
            DropTable("dbo.StockValuations");
        }
    }
}
