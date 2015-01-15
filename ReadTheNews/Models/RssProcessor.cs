using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using log4net;
using System.Data.SqlClient;

namespace ReadTheNews.Models
{
    public class RssProcessor : IDisposable
    {
        public SyndicationFeed Channel { get; private set; }
        public bool IsChannelDownload { get; private set; }

        private RssContext db;
        private RssChannel currentChannel;
        private string rssChannelUrl;
        private bool IsNewContent;        

        private string __sql;
        private List<SqlParameter> __sqlParameters = new List<SqlParameter>();

        public static ILog Logger { get; private set; }

        private RssProcessor() 
        {
            Logger = LogManager.GetLogger("RssProcessor");
            db = new RssContext();

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

        public RssChannel GetLatestNews()
        {
            if (!this.IsChannelDownload)
            {
                throw new ChannelNotDownloadException();
            }

            if (currentChannel == null)
                this.GetRssChannel();

            DateTime latestUpdate = new DateTime();
            if (this.Channel.LastUpdatedTime.DateTime != new DateTime())
                latestUpdate = this.Channel.LastUpdatedTime.DateTime;
            else
                latestUpdate = this.Channel.Items.First() != null ?
                    this.Channel.Items.First().PublishDate.DateTime : new DateTime();

            if (currentChannel.PubDate == latestUpdate && currentChannel.RssItems.Count > 0)
                return currentChannel;

            this.DeleteOldRssItems();

            currentChannel.RssItems = this.GetRssItemList();

            return currentChannel;
        }



        private void GetRssChannel()
        {
            if (!this.IsChannelDownload)
                throw new ChannelNotDownloadException();

            currentChannel = new RssChannel();

            currentChannel.Title = Channel.Title != null ? Channel.Title.Text : "no";
            currentChannel.Language = !String.IsNullOrEmpty(Channel.Language) ? Channel.Language : "no";
            currentChannel.Link = rssChannelUrl;
            currentChannel.Description = Channel.Description != null ? Channel.Description.Text : "no";
            currentChannel.ImageSrc = Channel.ImageUrl != null ? Channel.ImageUrl.ToString() : "no";
            currentChannel.PubDate = Channel.LastUpdatedTime.DateTime != new DateTime() ? 
                Channel.LastUpdatedTime.DateTime : 
                    Channel.Items.FirstOrDefault() != null ? 
                        Channel.Items.FirstOrDefault().PublishDate.DateTime : DateTime.Now;

            // Query executes in GetRssItemList()
            this.AddRssChannelInDb(currentChannel);            

            currentChannel.RssItems = this.GetRssItemList();
        }

        private List<RssItem> GetRssItemList(bool downloadByForce = false)
        {
            if (Channel == null || currentChannel == null)
                throw new ChannelNotDownloadException();
            if (!CheckNewContent(Channel.Items.First()) && currentChannel.RssItems.Count > 0 && !downloadByForce)
                return currentChannel.RssItems.ToList();

            var rssItems = new List<RssItem>(Channel.Items.Count());

            foreach (SyndicationItem item in Channel.Items)
            {
                RssItem rssItem = GetRssItem(item);
                if (rssItem != null)
                    rssItems.Add(rssItem);
                if (__sqlParameters.Count > 2000)
                    this.ExecuteQuery();
            }

            currentChannel.PubDate = Channel.LastUpdatedTime.DateTime != new DateTime() ?
                Channel.LastUpdatedTime.DateTime :
                    Channel.Items.FirstOrDefault() != null ?
                        Channel.Items.FirstOrDefault().PublishDate.DateTime : DateTime.Now;

            this.UpdateRssChannelPubDate(currentChannel);

            this.ExecuteQuery();

            return rssItems;
        }

        private RssItem GetRssItem(SyndicationItem item)
        {
            RssItem newRssItem = new RssItem();

            newRssItem.Title = item.Title != null ? item.Title.Text : "no";
            newRssItem.Description = item.Summary != null ?item.Summary.Text : "no";
            newRssItem.Date = item.PublishDate.Date;
            newRssItem.Link = !String.IsNullOrEmpty(item.Id) ? item.Id : "no";
            newRssItem.ImageSrc = "no";

            if (currentChannel == null)
                return null;

            newRssItem.RssChannel = currentChannel;
            //тестировать
            var categories = new List<RssCategory>(item.Categories.Count);

            foreach (SyndicationCategory category in item.Categories)
            {
                categories.Add(new RssCategory { Name = category.Name });
            }
            categories = categories.Distinct(new RssCategoryEqualityComparer()).ToList();
            foreach (RssCategory category in categories)
            {
                this.AddRssCategoryInDb(category);
            }
            newRssItem.RssCategories = categories;

            this.AddRssItemInDb(newRssItem);

            this.ExecuteQuery();

            return newRssItem;            
        }

        private bool CheckNewContent(SyndicationItem item)
        {
            if (item == null)
            {
                this.IsNewContent = false;
                return this.IsNewContent;
            }
            RssItem tempItem = db.RssItems.AsNoTracking().Where(i => i.Title == item.Title.Text).SingleOrDefault();
            if (tempItem != null)
            {
                this.IsNewContent = false;
                return this.IsNewContent;
            }
            this.IsNewContent = true;

            return this.IsNewContent;
        }

        

        private void AddRssCategoryInDb(RssCategory category)
        {
            string parameterName = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterName, category.Name));

            __sql += " INSERT INTO [dbo].[RssCategories](Name) " +
                     " VALUES (" + parameterName + "); ";            
        }

        private void AddRssChannelInDb(RssChannel channel)
        {
            string parameterTitle = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterTitle, channel.Title));

            string parameterLang = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterLang, channel.Language));

            string parameterLink = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterLink, channel.Link));

            string parameterDesc = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDesc, channel.Description));

            string parameterImgSrc = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterImgSrc, channel.ImageSrc));

            string parameterDate = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDate, channel.PubDate));

            __sql += " INSERT INTO [dbo].[RssChannels]([Title], [Language], [Link], [Description], [ImageSrc], [PubDate]) " +
                     " VALUES (" + parameterTitle + ", " + 
                                   parameterLang + ", " + 
                                   parameterLink + ", "+ 
                                   parameterDesc + ", " + 
                                   parameterImgSrc + ", " + 
                                   parameterDate + "); ";
        }

        private void AddRssItemInDb(RssItem item)
        {
            string parameterRssChannelTitle = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterRssChannelTitle, item.RssChannel.Title));

            string parameterTitle = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterTitle, item.Title));

            string parameterLink = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterLink, item.Link));

            string parameterDesc = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDesc, item.Description));

            string parameterDate = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDate, item.Date));

            string parameterImgSrc = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterImgSrc, item.ImageSrc));            

            __sql += " EXECUTE [dbo].[AddRssItem] " + parameterRssChannelTitle + ", " + 
                                                      parameterTitle + ", " + 
                                                      parameterLink + ", " + 
                                                      parameterDesc + ", " + 
                                                      parameterDate + ", " + 
                                                      parameterImgSrc + "; ";

            foreach (RssCategory category in item.RssCategories)
            {
                string parameterCategoryName = "@parameter" + __sqlParameters.Count;
                __sqlParameters.Add(new SqlParameter(parameterCategoryName, category.Name));
                __sql += " EXECUTE [dbo].[AddRssItemRssCategories] " + parameterTitle + ", "
                                                                     + parameterCategoryName + "; ";
            }

            //this.ExecuteQuery();
        }

        private void UpdateRssChannelPubDate(RssChannel channel)
        {
            string parameterDate = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDate, channel.PubDate));

            string parameterTitle = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterTitle, channel.Title));

            __sql += " UPDATE [dbo].[RssChannels] " + 
                     " SET [PubDate] = " + parameterDate +
                     " WHERE [Title] = " + parameterTitle + "; ";
        }

        private void ExecuteQuery()
        {
            db.Database.ExecuteSqlCommand(__sql, __sqlParameters.ToArray());
            __sql = "";
            __sqlParameters.Clear();
        }

        private void DeleteOldRssItems()
        {
            int id = currentChannel.Id;
            string parameterChannelId = "@parameter" + __sqlParameters.Count;
            SqlParameter channelId = new SqlParameter(parameterChannelId, id);

            __sql += " DELETE FROM [dbo].[RssItems] " +
                         " WHERE [dbo].[RssItems].[RssChannelId] = " + parameterChannelId + ";";

            __sqlParameters.Add(channelId);
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
}

public class ChannelNotDownloadException : Exception
{
    public ChannelNotDownloadException() : base("Rss-канал не был загружен") { }
}

