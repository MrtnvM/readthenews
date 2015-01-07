using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using log4net;
using System.Threading.Tasks;

namespace ReadTheNews.Models
{
    public class RssProcessor
    {
        public SyndicationFeed Channel { get; private set; }
        public bool IsChannelDownload { get; private set; }
        private RssChannel currentChannel;
        private RssContext db = new RssContext();

        private RssProcessor() 
        {
            IsChannelDownload = false;        
        }

        public static async Task<RssProcessor> GetRssProcessorAsync(string url)
        {
            RssProcessor processor = new RssProcessor();
            try
            {
                await Task.Run(() => {
                    XmlReader feedReader = XmlReader.Create(url);
                    processor.Channel = SyndicationFeed.Load(feedReader);
                    processor.IsChannelDownload = true;
                });
            }
            catch (Exception ex)
            {
                ex.LogException(typeof(RssProcessor), "error");
                processor.IsChannelDownload = false;
            }
            return processor;
        }

        public static async Task<RssProcessor> GetRssProcessorAsync(RssChannel rssChannel)
        {
            var processor = await GetRssProcessorAsync(rssChannel.Link);

            if (!processor.IsChannelDownload)
                processor.currentChannel = rssChannel;

            return processor;
        }

        public async Task<RssChannel> GetRssChannelAsync()
        {
            var newRssChannel = new RssChannel();

            if (!this.IsChannelDownload)
                throw new Exception("Rss-канал не был загружен");

            newRssChannel.Title = Channel.Title.Text;
            newRssChannel.Language = Channel.Language;
            newRssChannel.Link = Channel.Links[0] != null ? Channel.Links[0].Uri.ToString() : "";
            newRssChannel.Description = Channel.Description.Text;
            newRssChannel.ImageSrc = Channel.ImageUrl != null ? Channel.ImageUrl.ToString() : "";
            newRssChannel.PubDate = Channel.LastUpdatedTime.DateTime != new DateTime() ? 
                Channel.LastUpdatedTime.DateTime : 
                    Channel.Items.FirstOrDefault() != null ? 
                        Channel.Items.FirstOrDefault().PublishDate.DateTime : DateTime.Now;

            if (!db.RssChannels.Any(c => c.Title == newRssChannel.Title))
                db.RssChannels.Add(newRssChannel);
            await db.SaveChangesAsync();

            currentChannel = await Task.Run(() => db.RssChannels.Where<RssChannel>(c => c.Title == newRssChannel.Title).Single());

            var rssItems = await GetRssItemListAsync();
            newRssChannel.RssItems = rssItems;
            db.Entry(currentChannel).State = System.Data.Entity.EntityState.Modified;
            await db.SaveChangesAsync();

            return newRssChannel;
        }

        public async Task<List<RssItem>> GetRssItemListAsync()
        {
            var rssItems = new List<RssItem>(Channel.Items.Count());

            foreach (SyndicationItem item in Channel.Items)
            {
                RssItem rssItem = await GetRssItemAsync(item);
                rssItems.Add(rssItem);
            }

            return rssItems;
        }

        private async Task<RssItem> GetRssItemAsync(SyndicationItem item)
        {
            RssItem newRssItem = new RssItem();

            newRssItem.Title = item.Title.Text;
            newRssItem.Description = item.Summary.Text;
            newRssItem.Date = item.PublishDate.Date;
            newRssItem.Link = item.Id;
            newRssItem.RssChannelId = currentChannel.Id;
            newRssItem.RssChannel = currentChannel;

            var categories = new List<RssCategory>();
            foreach (var c in item.Categories)
            {
                categories.Add(new RssCategory { Name = c.Name });
            }

            var newCategories = new List<RssCategory>();
            
            categories.ForEach(c =>
            {
                if (!db.RssCategories.Any(category => c.Name == category.Name))
                    newCategories.Add(c);
            });
            
            db.RssCategories.AddRange(newCategories);
            await db.SaveChangesAsync();
            newRssItem.Categories = newCategories;

            if (!db.RssItems.Any(i => i.Title == newRssItem.Title))
            {
                db.RssItems.Add(newRssItem);
                await db.SaveChangesAsync();
            }

            return newRssItem;            
        }
    }
}