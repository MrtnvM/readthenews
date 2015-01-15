CREATE TABLE [dbo].[RssItems] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Title]         NVARCHAR (120) NOT NULL,
    [Link]          NVARCHAR (150) NOT NULL,
    [Description]   NVARCHAR (500) NOT NULL,
    [Date]          DATETIME       NOT NULL,
    [ImageSrc]      NVARCHAR (150) NULL,
    [RssChannelId]  INT            NOT NULL,
    [RssCategoryId] INT            NOT NULL,
    CONSTRAINT [PK_dbo.RssItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.RssItems_dbo.RssChannels_RssChannelId] FOREIGN KEY ([RssChannelId]) REFERENCES [dbo].[RssChannels] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.RssItems_dbo.RssCategories_RssCategoryId] FOREIGN KEY ([RssCategoryId]) REFERENCES [dbo].[RssCategories] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_RssChannelId]
    ON [dbo].[RssItems]([RssChannelId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_RssCategoryId]
    ON [dbo].[RssItems]([RssCategoryId] ASC);


GO
CREATE TRIGGER [Trg_RssItems_iofi_validate]
	ON [dbo].[RssItems]
	INSTEAD OF INSERT
	AS
	
	IF @@ROWCOUNT = 0 RETURN

	IF EXISTS (SELECT TOP(1) * FROM inserted AS I WHERE I.Title IN (SELECT Title FROM RssItems))
		INSERT INTO RssItemsLog([Title], [Link], [Description], [Date], [ImageSrc], [RssChannelId], [RssCategoryId])
			SELECT I.[Title], I.[Link], I.[Description], I.[Date], I.[ImageSrc], I.[RssChannelId], I.[RssCategoryId]
			FROM inserted AS I
			WHERE I.[Title] IN (SELECT [Title] FROM RssItems)

	INSERT INTO RssItems([Title], [Link], [Description], [Date], [ImageSrc], [RssChannelId], [RssCategoryId])
		SELECT I.[Title], I.[Link], I.[Description], I.[Date], I.[ImageSrc], I.[RssChannelId], I.[RssCategoryId]
		FROM inserted AS I
		WHERE I.[Title] NOT IN (SELECT [Title] FROM RssItems)