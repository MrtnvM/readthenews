using System;
using System.Collections.Generic;
using System.Linq;
using ReadTheNews.Models;
using System.Data.SqlClient;

namespace ReadTheNews.Helpers
{
    public class RssDataHelper
    {
        private RssContext db;
        private string __sql;
        private List<SqlParameter> __sqlParameters;

        public int CountOfSqlParameters
        {
            get
            {
                return __sqlParameters.Count;
            }
        }

        public RssDataHelper()
        {
            db = new RssContext();
            __sqlParameters = new List<SqlParameter>();
        }

        public void AddRssCategoryInDb(RssCategory category)
        {
            string parameterName = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterName, category.Name));

            __sql += " INSERT INTO [dbo].[RssCategories](Name) " +
                     " VALUES (" + parameterName + "); ";
        }

        public void AddRssChannelInDb(RssChannel channel)
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
                                   parameterLink + ", " +
                                   parameterDesc + ", " +
                                   parameterImgSrc + ", " +
                                   parameterDate + "); ";
        }

        public void AddRssItemInDb(RssItem item)
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
        }

        public void UpdateRssChannelPubDate(RssChannel channel)
        {
            string parameterDate = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterDate, channel.PubDate));

            string parameterTitle = "@parameter" + __sqlParameters.Count;
            __sqlParameters.Add(new SqlParameter(parameterTitle, channel.Title));

            __sql += " UPDATE [dbo].[RssChannels] " +
                     " SET [PubDate] = " + parameterDate +
                     " WHERE [Title] = " + parameterTitle + "; ";
        }

        public void ExecuteQuery()
        {
            db.Database.ExecuteSqlCommand(__sql, __sqlParameters.ToArray());
            __sql = "";
            __sqlParameters.Clear();
        }

        public void DeleteOldRssItems(RssChannel channel)
        {
            int id = channel.Id;
            string parameterChannelId = "@parameter" + __sqlParameters.Count;
            SqlParameter channelId = new SqlParameter(parameterChannelId, id);
            __sqlParameters.Add(channelId);

            DateTime yesterday = DateTime.Now.AddDays(-1).Date;
            string parameterYesterdayDate = "@parameter" + __sqlParameters.Count;
            SqlParameter yesterdayDate = new SqlParameter(parameterYesterdayDate, yesterday);
            __sqlParameters.Add(yesterdayDate);

            __sql += " DELETE FROM [dbo].[RssItems] " +
                         " WHERE [RssChannelId] = " + parameterChannelId + " AND " +
                               " [Date] < " + parameterYesterdayDate + " ;";
        }
    }
}