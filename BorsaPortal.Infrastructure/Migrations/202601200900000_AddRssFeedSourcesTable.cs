namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddRssFeedSourcesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RssFeedSources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        Description = c.String(),
                        FeedUrl = c.String(nullable: false, maxLength: 500),
                        IsActive = c.Boolean(nullable: false),
                        LastFetchedAt = c.DateTime(),
                        FetchIntervalMinutes = c.Int(nullable: false),
                        SectorId = c.Int(),
                        ItemsFetchedCount = c.Int(nullable: false),
                        IncludeKeywords = c.String(),
                        ExcludeKeywords = c.String(),
                        AutoPublish = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sectors", t => t.SectorId)
                .Index(t => t.SectorId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.RssFeedSources", "SectorId", "dbo.Sectors");
            DropIndex("dbo.RssFeedSources", new[] { "SectorId" });
            DropTable("dbo.RssFeedSources");
        }
    }
}
