CREATE TRIGGER [trg_RssChannels_iofi_validate]
	ON [dbo].[RssChannels]
	INSTEAD OF INSERT
	AS
	
	IF @@ROWCOUNT = 0 RETURN

	IF EXISTS (SELECT TOP(1) * FROM inserted AS I WHERE I.Title IN (SELECT Title FROM RssChannels))
		INSERT INTO RssChannelsLog([Title], [Language], [Link], [Description], [ImageSrc], [PubDate])
			SELECT I.Title, I.[Language], I.Link, I.[Description], I.ImageSrc, I.PubDate
			FROM inserted AS I
			WHERE I.Title IN (SELECT Title FROM RssChannels)

	INSERT INTO RssChannels ([Title], [Language], [Link], [Description], [ImageSrc], [PubDate])
		SELECT * 
		FROM inserted AS I
		WHERE I.Title NOT IN (SELECT Title FROM RssChannels)

GO
