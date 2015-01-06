using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using log4net;

namespace ReadTheNews.Models.RssModels
{
    public class RssProcessor
    {
        private SyndicationFeed channel;
        private RssChannel currentChannel;
        private RssContext db = new RssContext();

        private RssProcessor() { }

        public static RssProcessor GetRssProcessor(string url)
        {
            RssProcessor processor = new RssProcessor();
            try
            {
                XmlReader feedReader = XmlReader.Create(url);
                processor.channel = SyndicationFeed.Load(feedReader);
            }
            catch (Exception ex)
            {
                ex.LogException(typeof(RssProcessor), "error");
            }
            return processor;
        }

        public RssChannel GetRssChannel()
        {
            var newRssChannel = new RssChannel();

            newRssChannel.Title = channel.Title.Text;
            newRssChannel.Language = channel.Language;
            newRssChannel.Link = channel.Links[0] != null ? channel.Links[0].Uri.ToString() : "";
            newRssChannel.Description = channel.Description.Text;
            newRssChannel.ImageSrc = channel.ImageUrl != null ? channel.ImageUrl.ToString() : "";

            currentChannel = newRssChannel;

            newRssChannel.RssItems = GetRssItemList();
            db.RssChannels.Add(newRssChannel);
            db.SaveChanges();

            return newRssChannel;
        }

        private List<RssItem> GetRssItemList()
        {
            var rssItems = new List<RssItem>(channel.Items.Count());

            foreach (SyndicationItem item in channel.Items.Take(10))
            {
                rssItems.Add(GetRssItem(item));
            }

            return rssItems;

        }

        private RssItem GetRssItem(SyndicationItem item)
        {
            RssItem newRssItem = new RssItem();

            newRssItem.Title = item.Title.Text;
            newRssItem.Description = item.Summary.Text;
            newRssItem.Date = item.PublishDate.Date;
            newRssItem.Link = item.Id;

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
            newRssItem.Categories = newCategories;

            db.RssItems.Add(newRssItem);

            return newRssItem;
        }
    }
}