CREATE TABLE [dbo].[RssChanelsLog](
	[LogId]		  INT            IDENTITY (1, 1) NOT NULL,
	[Id]          INT            NULL,
    [Title]       NVARCHAR       NULL,
    [Language]    NVARCHAR       NULL,
    [Link]        NVARCHAR       NULL,
    [Description] NVARCHAR       NULL,
    [ImageSrc]    NVARCHAR       NULL,
    [PubDate]     DATETIME       NULL,
	[ErrorDate]   DATETIME       NOT NULL DEFAULT (getdate()),
    CONSTRAINT [PK_dbo.RssChannelsLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
);