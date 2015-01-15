CREATE TABLE [dbo].[RssItemsLog] (
    [LogId]         INT            IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR (120) NULL,
    [Link]          NVARCHAR (150) NULL,
    [Description]   NVARCHAR (50)  NULL,
    [Date]          DATETIME       NULL,
    [ImageSrc]      NVARCHAR (150) NULL,
    [RssChannelId]  INT            NULL,
    [RssCategoryId] INT            NULL,
    [ErrorDate]     DATETIME       DEFAULT (getdate()) NOT NULL,
    PRIMARY KEY CLUSTERED ([LogId] ASC)
);

