-- Create RssFeedSources table for news automation
-- Run this script in your SQL Server database

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RssFeedSources]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RssFeedSources](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](max) NULL,
        [FeedUrl] [nvarchar](500) NOT NULL,
        [IsActive] [bit] NOT NULL,
        [LastFetchedAt] [datetime] NULL,
        [FetchIntervalMinutes] [int] NOT NULL,
        [SectorId] [int] NULL,
        [ItemsFetchedCount] [int] NOT NULL,
        [IncludeKeywords] [nvarchar](max) NULL,
        [ExcludeKeywords] [nvarchar](max) NULL,
        [AutoPublish] [bit] NOT NULL,
        [CreatedAt] [datetime] NOT NULL,
        [UpdatedAt] [datetime] NULL,
        [IsDeleted] [bit] NOT NULL,
        CONSTRAINT [PK_dbo.RssFeedSources] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    -- Create foreign key to Sectors table
    ALTER TABLE [dbo].[RssFeedSources]  WITH CHECK ADD CONSTRAINT [FK_dbo.RssFeedSources_dbo.Sectors_SectorId]
        FOREIGN KEY([SectorId]) REFERENCES [dbo].[Sectors] ([Id]);

    -- Create index on SectorId
    CREATE NONCLUSTERED INDEX [IX_SectorId] ON [dbo].[RssFeedSources]([SectorId] ASC);

    PRINT 'RssFeedSources table created successfully!';
END
ELSE
BEGIN
    PRINT 'RssFeedSources table already exists!';
END
GO

-- Insert some sample RSS feed sources with verified working URLs
IF NOT EXISTS (SELECT * FROM [dbo].[RssFeedSources])
BEGIN
    INSERT INTO [dbo].[RssFeedSources]
        ([Name], [Description], [FeedUrl], [IsActive], [FetchIntervalMinutes], [SectorId], [ItemsFetchedCount], [AutoPublish], [CreatedAt], [IsDeleted])
    VALUES
        (N'NTV Ekonomi', N'NTV ekonomi haberleri', N'https://www.ntv.com.tr/ekonomi.rss', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Habertürk Ekonomi', N'Habertürk ekonomi haberleri', N'https://www.haberturk.com/rss/ekonomi.xml', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Bigpara', N'Hürriyet Bigpara borsa ve ekonomi haberleri', N'https://bigpara.hurriyet.com.tr/rss/', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Investing.com Forex', N'Investing.com döviz haberleri', N'https://tr.investing.com/rss/forex.rss', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Investing.com Hisse', N'Investing.com hisse senedi haberleri', N'https://tr.investing.com/rss/stock.rss', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Dünya Gazetesi', N'Dünya Gazetesi ekonomi haberleri', N'https://www.dunya.com/rss?dunya', 1, 30, NULL, 0, 0, GETDATE(), 0),
        (N'Bloomberg HT', N'Bloomberg HT ekonomi haberleri', N'https://www.bloomberght.com/rss', 1, 30, NULL, 0, 0, GETDATE(), 0);

    PRINT 'Sample RSS feed sources added!';
END
GO
