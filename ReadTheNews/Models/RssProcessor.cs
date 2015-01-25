using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.ServiceModel.Syndication;
using System.Xml;
using log4net;
using System.Data.SqlClient;
using ReadTheNews.Helpers;

namespace ReadTheNews.Models
{
    public class RssProcessor : IDisposable
    {
        public SyndicationFeed Channel { get; private set; }
        public bool IsChannelDownload { get; private set; }

        private RssContext db;
        private RssDataHelper dataHelper;
        private RssChannel currentChannel;
        private string rssChannelUrl;
        private bool IsNewContent;
        private RssItem _lastRssItemInDb;
        

        public static ILog Logger { get; private set; }

        private RssProcessor()
        {
            Logger = LogManager.GetLogger("RssProcessor");
            db = new RssContext();
            dataHelper = new RssDataHelper();

            IsChannelDownload = false;
            IsNewContent = true;
        }

        public static RssProcessor GetRssProcessor(string url)
        {
            RssProcessor processor = new RssProcessor();
            try
            {
                XmlReader feedReader = XmlReader.Create(url);
                processor.Channel = SyndicationFeed.Load(feedReader);
                processor.IsChannelDownload = true;
                processor.rssChannelUrl = url;
            }
            catch (Exception ex)
            {
                ex.LogException(typeof(RssProcessor), "error");
                processor.IsChannelDownload = false;
            }
            return processor;
        }

        public static RssProcessor GetRssProcessor(RssChannel rssChannel)
        {
            var processor = GetRssProcessor(rssChannel.Link);

            processor.currentChannel = rssChannel;

            return processor;
        }

        public static RssProcessor GetRssProcessor(int rssChannelId)
        {
            RssChannel tempChannel;
            using (var db = new RssContext())
            {
                tempChannel = db.RssChannels.Find(rssChannelId);
            }
            RssProcessor processor = GetRssProcessor(tempChannel.Link);
            processor.currentChannel = tempChannel;

            return processor;
        }

        public RssChannel GetLatestNews(string userId)
        {
            if (!this.IsChannelDownload)
                throw new ChannelNotDownloadException();

            if (currentChannel == null)
            {
                this.GetRssChannel(userId);
                return currentChannel;
            }

            DateTime latestUpdate = new DateTime();
            if (this.Channel.LastUpdatedTime.DateTime != new DateTime())
                latestUpdate = this.Channel.LastUpdatedTime.DateTime;
            else
                latestUpdate = this.Channel.Items.First() != null ?
                    this.Channel.Items.First().PublishDate.DateTime : new DateTime();

            if (currentChannel.PubDate == latestUpdate && currentChannel.RssItems.Count > 0)
                return currentChannel;

            dataHelper.DeleteOldRssItems(currentChannel);

            currentChannel.RssItems = this.GetRssItemList(userId);

            return currentChannel;
        }



        private void GetRssChannel(string userId)
        {
            if (!this.IsChannelDownload)
                throw new ChannelNotDownloadException();

            currentChannel = RssParseHelper.ParseChannel(Channel, rssChannelUrl);   
            // Query executes in GetRssItemList()
            dataHelper.AddRssChannelInDb(currentChannel);

            currentChannel.RssItems = this.GetRssItemList(userId);
        }

        private List<RssItem> GetRssItemList(string userId)
        {
            if (Channel == null || currentChannel == null)
                throw new ChannelNotDownloadException();
            if (!CheckNewContent(Channel.Items.First()) && currentChannel.RssItems.Count > 0)
                return currentChannel.RssItems.ToList();

            var rssItems = new List<RssItem>(Channel.Items.Count());
            DateTime yesterday = DateTime.Today.AddDays(-1).Date;
            foreach (SyndicationItem item in Channel.Items)
            {
                if (_lastRssItemInDb != null && item.Title.Text == _lastRssItemInDb.Title)
                    break;

                if (item.PublishDate < yesterday)
                    break;

                RssItem rssItem = GetRssItem(item);                
                
                if (rssItem != null)
                    rssItems.Add(rssItem);

                if (dataHelper.CountOfSqlParameters > 2000)
                    dataHelper.ExecuteQuery();
            }

            currentChannel.PubDate = Channel.LastUpdatedTime.DateTime != new DateTime() ?
                Channel.LastUpdatedTime.DateTime :
                    Channel.Items.FirstOrDefault() != null ?
                        Channel.Items.FirstOrDefault().PublishDate.DateTime : DateTime.Now;
            
            List<RssItem> rssItemsInDb = dataHelper.GetFiltredNews(currentChannel.Id, userId);

            dataHelper.UpdateRssChannelPubDate(currentChannel);

            dataHelper.ExecuteQuery();
            
            rssItems.AddRange(rssItemsInDb);
            return rssItems;
        }

        private RssItem GetRssItem(SyndicationItem item)
        {
            RssItem newRssItem = RssParseHelper.ParseItem(item, currentChannel);

            dataHelper.AddRssItemInDb(newRssItem);

            return newRssItem;
        }

        private bool CheckNewContent(SyndicationItem item)
        {
            if (item == null)
            {
                this.IsNewContent = false;
                return this.IsNewContent;
            }
            SqlParameter channelId = new SqlParameter("@channelId", currentChannel.Id);
            string sql = " SELECT TOP(1) * " +
                         " FROM [RssItems] " +
                         " WHERE [RssChannelId] = @channelId; ";
            _lastRssItemInDb = db.Database.SqlQuery<RssItem>(sql, channelId).FirstOrDefault();

            if (_lastRssItemInDb != null && _lastRssItemInDb.Title == item.Title.Text)
            {
                this.IsNewContent = false;
                return this.IsNewContent;
            }
            this.IsNewContent = true;

            return this.IsNewContent;
        }

        public void Dispose()
        {
            db.Dispose();
        }

        ~RssProcessor()
        {
            this.Dispose();
        }
    }

    public class RssCategoryEqualityComparer : IEqualityComparer<RssCategory>
    {
        public bool Equals(RssCategory category1, RssCategory category2)
        {
            if (category1.Name.ToLower() == category2.Name.ToLower())
                return true;
            return false;
        }

        public int GetHashCode(RssCategory category)
        {
            return category.Name.ToLower().GetHashCode();
        }
    }


    public class ChannelNotDownloadException : Exception
    {
        public ChannelNotDownloadException() : base("Rss-канал не был загружен") { }
    }

}