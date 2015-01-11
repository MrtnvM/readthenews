using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using log4net;
using EntityFramework.Extensions;
using System.Data.Entity.Infrastructure;
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
        private List<SqlParameter> __sqlParametrs = new List<SqlParameter>();

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

            this.DeleteRssItemsByChannelId(currentChannel.Id);

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

            var firstCategory = item.Categories.Count > 0 ? item.Categories[0] : null;
            if (firstCategory == null)
                return null;
            
            RssCategory tempCategory = new RssCategory { Name = firstCategory.Name };
            this.AddRssCategoryInDb(tempCategory);

            newRssItem.RssCategory = tempCategory;

            this.AddRssItemInDb(newRssItem);

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
            string parameterName = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterName, category.Name));

            __sql += " INSERT INTO [dbo].[RssCategories](Name) " +
                     " VALUES (" + parameterName + "); ";            
        }

        private void AddRssChannelInDb(RssChannel channel)
        {
            string parameterTitle = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterTitle, channel.Title));

            string parameterLang = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterLang, channel.Language));

            string parameterLink = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterLink, channel.Link));

            string parameterDesc = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterDesc, channel.Description));

            string parameterImgSrc = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterImgSrc, channel.ImageSrc));

            string parameterDate = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterDate, channel.PubDate));

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
            string parameterRssChannelTitle = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterRssChannelTitle, item.RssChannel.Title));

            string parameterRssCategoryName = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterRssCategoryName, item.RssCategory.Name));

            string parameterTitle = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterTitle, item.Title));

            string parameterLink = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterLink, item.Link));

            string parameterDesc = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterDesc, item.Description));

            string parameterDate = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterDate, item.Date));

            string parameterImgSrc = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterImgSrc, item.ImageSrc));            

            __sql += " EXECUTE [dbo].[AddRssItem] " + parameterRssChannelTitle + ", " + 
                                                      parameterRssCategoryName + ", " + 
                                                      parameterTitle + ", " + 
                                                      parameterLink + ", " + 
                                                      parameterDesc + ", " + 
                                                      parameterDate + ", " + 
                                                      parameterImgSrc + "; ";

            //this.ExecuteQuery();
        }

        private void UpdateRssChannelPubDate(RssChannel channel)
        {
            string parameterDate = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterDate, channel.PubDate));

            string parameterTitle = "@parameter" + __sqlParametrs.Count;
            __sqlParametrs.Add(new SqlParameter(parameterTitle, channel.Title));

            __sql += " UPDATE [dbo].[RssChannels] " + 
                     " SET [PubDate] = " + parameterDate +
                     " WHERE [Title] = " + parameterTitle + "; ";
        }

        private void ExecuteQuery()
        {
            db.Database.ExecuteSqlCommand(__sql, __sqlParametrs.ToArray());
            __sql = "";
            __sqlParametrs.Clear();
        }

        private void DeleteRssItemsByChannelId(int id)
        {
            string parameterChannelId = "@parameter" + __sqlParametrs.Count;
            SqlParameter channelId = new SqlParameter(parameterChannelId, id);

            __sql += " DELETE FROM [dbo].[RssItems] " +
                         " WHERE [dbo].[RssItems].[RssChannelId] = " + parameterChannelId + ";";

            __sqlParametrs.Add(channelId);
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
}

public class ChannelNotDownloadException : Exception
{
    public ChannelNotDownloadException() : base("Rss-канал не был загружен") { }
}