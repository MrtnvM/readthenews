﻿using System;
using System.Collections.Generic;
using System.Linq;
using ReadTheNews.Models;
using System.Data.SqlClient;
using log4net;

namespace ReadTheNews.Helpers
{
    public class RssDataHelper : IDisposable
    {
        private RssContext db;
        private string __sql;
        private List<SqlParameter> __sqlParameters;
        ILog logger = LogManager.GetLogger(typeof(RssDataHelper));

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

        public List<RssItem> GetFiltredNews(int channelId, string userId)
        {
            if (channelId == 0)
                throw new Exception("Канал не был загружен");

            if (String.IsNullOrEmpty(userId))
                throw new Exception("Пользователь не авторизован");

            SqlParameter parameterChannelId = new SqlParameter("@channelId", channelId);
            SqlParameter parameterUserId = new SqlParameter("@userId", userId);
            string sql = " SELECT * FROM [dbo].GetFiltredNews(@channelId, @userId); ";

            List<RssItem> news = db.RssItems.SqlQuery(sql, parameterChannelId, parameterUserId).ToList();
            
            return news;
        }

        public bool AddRssNewsToFavorite(int rssNewsId, string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Невозвожно добавить в список избранного новость от анонимного пользователя");
                if (rssNewsId <= 0)
                    throw new Exception("Некоректный идентификатор новости при добавлении в избранное");

                var parameterNewsId = new SqlParameter("@newsId", rssNewsId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " INSERT INTO [dbo].[FavoriteRssNews]([RssItemId], [UserId]) " +
                             " VALUES (@newsId, @userId); ";
                db.Database.ExecuteSqlCommand(sql, parameterNewsId, parameterUserId);
                return true;
            } catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public List<RssItem> GetFavoriteRssNews(string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Невозвожно получить список избранного анонимного пользователя");

                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT * FROM [dbo].[RssItems]  " +
                             " WHERE [Id] IN ( SELECT [RssItemId] " +
                                             " FROM [dbo].[FavoriteRssNews] " +
                                             " WHERE [UserId] = @userId) ";
                List<RssItem> favoriteNews = db.Database.SqlQuery<RssItem>(sql, parameterUserId).ToList();
                return favoriteNews;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}