namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateNewsEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.News", "Content", c => c.String());
            AddColumn("dbo.News", "IsFeatured", c => c.Boolean(nullable: false));
            AddColumn("dbo.News", "IsAutoFetched", c => c.Boolean(nullable: false));
            AddColumn("dbo.News", "SectorId", c => c.Int());
            CreateIndex("dbo.News", "SectorId");
            AddForeignKey("dbo.News", "SectorId", "dbo.Sectors", "Id");
            DropColumn("dbo.News", "Body");
        }
        
        public override void Down()
        {
            AddColumn("dbo.News", "Body", c => c.String());
            DropForeignKey("dbo.News", "SectorId", "dbo.Sectors");
            DropIndex("dbo.News", new[] { "SectorId" });
            DropColumn("dbo.News", "SectorId");
            DropColumn("dbo.News", "IsAutoFetched");
            DropColumn("dbo.News", "IsFeatured");
            DropColumn("dbo.News", "Content");
        }
    }
}
