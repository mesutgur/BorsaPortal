using BorsaPortal.Domain.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;


namespace BorsaPortal.Infrastructure.Data
{
    public class BorsaPortalDbContext : IdentityDbContext<ApplicationUser>
    {
        public BorsaPortalDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static BorsaPortalDbContext Create()
        {
            return new BorsaPortalDbContext();
        }

        // Domain Entities
        public DbSet<Company> Companies { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<MarketIndex> MarketIndices { get; set; }

        // Blog Entities
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Content Entities
        public DbSet<KapNotification> KapNotifications { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<RssFeedSource> RssFeedSources { get; set; }

        // User Features
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<Watchlist> Watchlists { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

        // Valuation Entities
        public DbSet<ValuationMethod> ValuationMethods { get; set; }
        public DbSet<StockValuation> StockValuations { get; set; }
        public DbSet<ValuationCalculation> ValuationCalculations { get; set; }
        public DbSet<UserValuationRight> UserValuationRights { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity mappings
            ConfigureCompany(modelBuilder);
            ConfigureSector(modelBuilder);
            ConfigureMarketIndex(modelBuilder);
            ConfigureBlog(modelBuilder);
            ConfigureComment(modelBuilder);
            ConfigureKapNotification(modelBuilder);
            ConfigureNews(modelBuilder);
            ConfigureRssFeedSource(modelBuilder);
            ConfigureUserFeatures(modelBuilder);
            ConfigureValuation(modelBuilder);
        }

        private void ConfigureCompany(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .ToTable("Companies")
                .HasKey(c => c.Id);

            modelBuilder.Entity<Company>()
                .Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(10);

            modelBuilder.Entity<Company>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Company>()
                .Property(c => c.Website)
                .HasMaxLength(200);

            modelBuilder.Entity<Company>()
                .HasOptional(c => c.Sector)
                .WithMany()
                .HasForeignKey(c => c.SectorId);
        }

        private void ConfigureSector(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sector>()
                .ToTable("Sectors")
                .HasKey(s => s.Id);

            modelBuilder.Entity<Sector>()
                .Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Sector>()
                .Property(s => s.Code)
                .HasMaxLength(20);
        }

        private void ConfigureMarketIndex(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MarketIndex>()
                .ToTable("MarketIndices")
                .HasKey(m => m.Id);

            modelBuilder.Entity<MarketIndex>()
                .Property(m => m.Code)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<MarketIndex>()
                .Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<MarketIndex>()
                .Property(m => m.Description)
                .HasMaxLength(500);
        }

        private void ConfigureBlog(DbModelBuilder modelBuilder)
        {
            // BlogCategory
            modelBuilder.Entity<BlogCategory>()
                .ToTable("BlogCategories")
                .HasKey(c => c.Id);

            modelBuilder.Entity<BlogCategory>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<BlogCategory>()
                .Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(150);

            // BlogPost
            modelBuilder.Entity<BlogPost>()
                .ToTable("BlogPosts")
                .HasKey(p => p.Id);

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(300);

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(350);

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.SeoTitle)
                .HasMaxLength(150);

            modelBuilder.Entity<BlogPost>()
                .Property(p => p.SeoDescription)
                .HasMaxLength(300);

            modelBuilder.Entity<BlogPost>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BlogPost>()
                .HasOptional(p => p.Author)
                .WithMany()
                .HasForeignKey(p => p.AuthorId);

            // Tag
            modelBuilder.Entity<Tag>()
                .ToTable("Tags")
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tag>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Tag>()
                .Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(60);

            // PostTag (Many-to-Many junction table)
            modelBuilder.Entity<PostTag>()
                .ToTable("PostTags")
                .HasKey(pt => new { pt.PostId, pt.TagId });

            modelBuilder.Entity<PostTag>()
                .HasRequired(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PostTag>()
                .HasRequired(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .WillCascadeOnDelete(true);
        }

        private void ConfigureComment(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .ToTable("Comments")
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comment>()
                .Property(c => c.Text)
                .IsRequired();

            modelBuilder.Entity<Comment>()
                .Property(c => c.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserDisplayName)
                .HasMaxLength(100);

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserEmail)
                .HasMaxLength(100);

            modelBuilder.Entity<Comment>()
                .HasOptional(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasOptional(c => c.BlogPost)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .WillCascadeOnDelete(false);
        }

        private void ConfigureKapNotification(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KapNotification>()
                .ToTable("KapNotifications")
                .HasKey(k => k.Id);

            modelBuilder.Entity<KapNotification>()
                .Property(k => k.Title)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<KapNotification>()
                .Property(k => k.NotificationNumber)
                .HasMaxLength(50);

            modelBuilder.Entity<KapNotification>()
                .Property(k => k.SourceUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<KapNotification>()
                .HasRequired(k => k.Company)
                .WithMany()
                .HasForeignKey(k => k.CompanyId)
                .WillCascadeOnDelete(false);
        }

        private void ConfigureNews(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<News>()
                .ToTable("News")
                .HasKey(n => n.Id);

            modelBuilder.Entity<News>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<News>()
                .Property(n => n.Source)
                .HasMaxLength(200);

            modelBuilder.Entity<News>()
                .Property(n => n.SourceUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<News>()
                .Property(n => n.ImageUrl)
                .HasMaxLength(500);

            modelBuilder.Entity<News>()
                .HasOptional(n => n.Company)
                .WithMany()
                .HasForeignKey(n => n.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<News>()
                .HasOptional(n => n.Sector)
                .WithMany()
                .HasForeignKey(n => n.SectorId)
                .WillCascadeOnDelete(false);
        }

        private void ConfigureRssFeedSource(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RssFeedSource>()
                .ToTable("RssFeedSources")
                .HasKey(r => r.Id);

            modelBuilder.Entity<RssFeedSource>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<RssFeedSource>()
                .Property(r => r.FeedUrl)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<RssFeedSource>()
                .Property(r => r.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<RssFeedSource>()
                .HasOptional(r => r.Sector)
                .WithMany()
                .HasForeignKey(r => r.SectorId)
                .WillCascadeOnDelete(false);
        }

        private void ConfigureUserFeatures(DbModelBuilder modelBuilder)
        {
            // Portfolio
            modelBuilder.Entity<Portfolio>()
                .ToTable("Portfolios")
                .HasKey(p => p.Id);

            modelBuilder.Entity<Portfolio>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);

            // PortfolioItem
            modelBuilder.Entity<PortfolioItem>()
                .ToTable("PortfolioItems")
                .HasKey(p => p.Id);

            modelBuilder.Entity<PortfolioItem>()
                .HasRequired(p => p.Portfolio)
                .WithMany(po => po.Items)
                .HasForeignKey(p => p.PortfolioId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<PortfolioItem>()
                .HasRequired(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .WillCascadeOnDelete(false);

            // Watchlist
            modelBuilder.Entity<Watchlist>()
                .ToTable("Watchlists")
                .HasKey(w => w.Id);

            modelBuilder.Entity<Watchlist>()
                .HasRequired(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Watchlist>()
                .HasRequired(w => w.Company)
                .WithMany()
                .HasForeignKey(w => w.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Watchlist>()
                .Property(w => w.Notes)
                .HasMaxLength(500);

            // UserFavorite
            modelBuilder.Entity<UserFavorite>()
                .ToTable("UserFavorites")
                .HasKey(f => f.Id);

            modelBuilder.Entity<UserFavorite>()
                .HasRequired(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserFavorite>()
                .Property(f => f.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<UserFavorite>()
                .Property(f => f.Notes)
                .HasMaxLength(500);

            // UserPreference
            modelBuilder.Entity<UserPreference>()
                .ToTable("UserPreferences")
                .HasKey(p => p.Id);

            modelBuilder.Entity<UserPreference>()
                .HasRequired(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserPreference>()
                .Property(p => p.PreferredLanguage)
                .HasMaxLength(10);

            modelBuilder.Entity<UserPreference>()
                .Property(p => p.PreferredCurrency)
                .HasMaxLength(10);

            modelBuilder.Entity<UserPreference>()
                .Property(p => p.DailyDigestTime)
                .HasMaxLength(5);

            // UserNotification
            modelBuilder.Entity<UserNotification>()
                .ToTable("UserNotifications")
                .HasKey(n => n.Id);

            modelBuilder.Entity<UserNotification>()
                .HasRequired(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserNotification>()
                .Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<UserNotification>()
                .Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            modelBuilder.Entity<UserNotification>()
                .Property(n => n.NotificationType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<UserNotification>()
                .Property(n => n.RelatedEntityType)
                .HasMaxLength(50);

            modelBuilder.Entity<UserNotification>()
                .Property(n => n.ActionUrl)
                .HasMaxLength(500);
        }

        private void ConfigureValuation(DbModelBuilder modelBuilder)
        {
            // ValuationMethod
            modelBuilder.Entity<ValuationMethod>()
                .ToTable("ValuationMethods")
                .HasKey(v => v.Id);

            modelBuilder.Entity<ValuationMethod>()
                .Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ValuationMethod>()
                .Property(v => v.Description)
                .HasMaxLength(1000);

            modelBuilder.Entity<ValuationMethod>()
                .Property(v => v.MethodType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<ValuationMethod>()
                .Property(v => v.Formula)
                .HasMaxLength(2000);

            // StockValuation
            modelBuilder.Entity<StockValuation>()
                .ToTable("StockValuations")
                .HasKey(v => v.Id);

            modelBuilder.Entity<StockValuation>()
                .HasRequired(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockValuation>()
                .HasRequired(v => v.Company)
                .WithMany()
                .HasForeignKey(v => v.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockValuation>()
                .HasOptional(v => v.ValuationMethod)
                .WithMany(m => m.StockValuations)
                .HasForeignKey(v => v.ValuationMethodId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<StockValuation>()
                .Property(v => v.Notes)
                .HasMaxLength(2000);

            // ValuationCalculation
            modelBuilder.Entity<ValuationCalculation>()
                .ToTable("ValuationCalculations")
                .HasKey(c => c.Id);

            modelBuilder.Entity<ValuationCalculation>()
                .HasRequired(c => c.StockValuation)
                .WithMany(v => v.Calculations)
                .HasForeignKey(c => c.StockValuationId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ValuationCalculation>()
                .Property(c => c.CalculationType)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<ValuationCalculation>()
                .Property(c => c.CalculationName)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<ValuationCalculation>()
                .Property(c => c.Formula)
                .HasMaxLength(1000);

            modelBuilder.Entity<ValuationCalculation>()
                .Property(c => c.FormulaWithValues)
                .HasMaxLength(1000);

            // UserValuationRight
            modelBuilder.Entity<UserValuationRight>()
                .ToTable("UserValuationRights")
                .HasKey(r => r.Id);

            modelBuilder.Entity<UserValuationRight>()
                .HasRequired(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);
        }
    }
}
