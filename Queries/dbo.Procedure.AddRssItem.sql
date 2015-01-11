CREATE PROCEDURE [dbo].[AddRssItem]
	@channelTitle nvarchar (120),
	@categoryName nvarchar (15),
	@itemTitle nvarchar (120),
	@itemLink nvarchar (150),
	@itemDescription nvarchar(500),
	@itemDate datetime,
	@itemImageSrc nvarchar (150)
AS
	declare @categoryId int;
	declare @channelId int

	SET @categoryId = (
		SELECT [Id]
		FROM [dbo].[RssCategories] AS c
		WHERE [Id] = c.Id AND c.Name = @categoryName
	);

	SET @channelId = (
		SELECT [Id]
		FROM [dbo].[RssChannels] AS c
		WHERE [Id] = c.[Id] AND c.Title = @channelTitle
	);

	insert into RssItems ([Title], [Link], [Description], [Date], [RssChannelId], [RssCategoryId])
	values (@itemTitle, @itemLink, @itemDescription, @itemDate, @channelId, @categoryId);

GO
