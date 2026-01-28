namespace BorsaPortal.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 200),
                        Message = c.String(nullable: false, maxLength: 1000),
                        NotificationType = c.String(nullable: false, maxLength: 50),
                        RelatedEntityId = c.Int(),
                        RelatedEntityType = c.String(maxLength: 50),
                        IsRead = c.Boolean(nullable: false),
                        NotificationDate = c.DateTime(nullable: false),
                        ActionUrl = c.String(maxLength: 500),
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
            DropForeignKey("dbo.UserNotifications", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserNotifications", new[] { "UserId" });
            DropTable("dbo.UserNotifications");
        }
    }
}
