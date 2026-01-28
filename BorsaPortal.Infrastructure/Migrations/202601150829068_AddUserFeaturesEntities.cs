namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserFeaturesEntities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PortfolioItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PortfolioId = c.Int(nullable: false),
                        CompanyId = c.Int(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AverageBuyPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProfitLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ProfitLossPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LastPriceUpdate = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.Portfolios", t => t.PortfolioId, cascadeDelete: true)
                .Index(t => t.PortfolioId)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.Portfolios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        TotalValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalProfitLoss = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalProfitLossPercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserFavorites",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EntityType = c.String(nullable: false, maxLength: 50),
                        EntityId = c.Int(nullable: false),
                        AddedAt = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserPreferences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EmailNotificationsEnabled = c.Boolean(nullable: false),
                        SmsNotificationsEnabled = c.Boolean(nullable: false),
                        PushNotificationsEnabled = c.Boolean(nullable: false),
                        NotifyOnKapDisclosures = c.Boolean(nullable: false),
                        NotifyOnPriceChanges = c.Boolean(nullable: false),
                        NotifyOnNewsPublished = c.Boolean(nullable: false),
                        PreferredLanguage = c.String(maxLength: 10),
                        PreferredCurrency = c.String(maxLength: 10),
                        ItemsPerPage = c.Int(nullable: false),
                        PriceChangeAlertPercentage = c.Decimal(precision: 18, scale: 2),
                        DailyDigestEnabled = c.Boolean(nullable: false),
                        DailyDigestTime = c.String(maxLength: 5),
                        ProfileVisible = c.Boolean(nullable: false),
                        PortfolioVisible = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Watchlists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CompanyId = c.Int(nullable: false),
                        Notes = c.String(maxLength: 500),
                        AddedAt = c.DateTime(nullable: false),
                        TargetPrice = c.Decimal(precision: 18, scale: 2),
                        NotifyOnTargetPrice = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.CompanyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Watchlists", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Watchlists", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.UserPreferences", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserFavorites", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PortfolioItems", "PortfolioId", "dbo.Portfolios");
            DropForeignKey("dbo.Portfolios", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PortfolioItems", "CompanyId", "dbo.Companies");
            DropIndex("dbo.Watchlists", new[] { "CompanyId" });
            DropIndex("dbo.Watchlists", new[] { "UserId" });
            DropIndex("dbo.UserPreferences", new[] { "UserId" });
            DropIndex("dbo.UserFavorites", new[] { "UserId" });
            DropIndex("dbo.Portfolios", new[] { "UserId" });
            DropIndex("dbo.PortfolioItems", new[] { "CompanyId" });
            DropIndex("dbo.PortfolioItems", new[] { "PortfolioId" });
            DropTable("dbo.Watchlists");
            DropTable("dbo.UserPreferences");
            DropTable("dbo.UserFavorites");
            DropTable("dbo.Portfolios");
            DropTable("dbo.PortfolioItems");
        }
    }
}
