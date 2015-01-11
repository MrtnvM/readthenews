SELECT COUNT(*) AS BeforeTruncateCount 
FROM dbo.RssChannels;
GO


DELETE FROM dbo.RssChannels;

GO

SELECT COUNT(*) AS AfterTruncateCount 
FROM dbo.RssChannels;
GO