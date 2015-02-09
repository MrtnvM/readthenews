using System;
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
            try
            {
                db.Database.ExecuteSqlCommand(__sql, __sqlParameters.ToArray());
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
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

            string parameterTodayDate = "@parameter" + __sqlParameters.Count;
            SqlParameter todayDate = new SqlParameter(parameterTodayDate, DateTime.Now.Date);
            __sqlParameters.Add(todayDate);

            __sql += " DELETE FROM [dbo].[RssItems] " +
                         " WHERE [RssChannelId] = " + parameterChannelId + " AND " +
                               " [Date] < " + parameterYesterdayDate + " AND " +
                               " [RemoveDate] < " + parameterTodayDate + " ;";
        }

        public List<RssItem> GetFiltredRssNews(int channelId, string userId)
        {
            try
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
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public bool AddRssNewsToFavorite(int rssNewsId, string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Невозвожно добавить в список избранного новость от анонимного пользователя");
                if (rssNewsId <= 0)
                    throw new Exception("Некорректный идентификатор новости при добавлении в избранное");

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

        public List<RssItem> GetReadingList(string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Невозможно получить список для чтения анонимного пользователя");

                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT * FROM [dbo].[RssItems] " +
                             " WHERE [Id] IN ( SELECT [RssItemId] " +
                                             " FROM [dbo].[RssNewsForReadingLater]" +
                                             " WHERE [UserId] = @userId ) ";
                List<RssItem> readingList = db.Database.SqlQuery<RssItem>(sql, parameterUserId).ToList();
                return readingList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public bool AddRssNewsToReadingList(int rssNewsId, string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Невозможно добавить в список для чтения новость от анонимного пользователя");
                if (rssNewsId <= 0)
                    throw new Exception("Некоррекктный идентификатор при добавлении новости в список для чтения");

                var parameterNewsId = new SqlParameter("@newsId", rssNewsId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " INSERT INTO [dbo].[RssNewsForReadingLater]([RssItemId], [UserId]) " +
                             " VALUES (@newsId, @userId) ";
                db.Database.ExecuteSqlCommand(sql, parameterNewsId, parameterUserId);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public bool DeleteRssNewsFromUserNewsList(int rssNewsId, string userId)
        {
            try
            {
                if (rssNewsId <= 0)
                    throw new Exception("Некоректный идентификатор новости при ее удалении");
                if (userId == null)
                    throw new Exception("Аннонимный пользователь не может удалять новости из списка");

                var parameterNewsId = new SqlParameter("@newsId", rssNewsId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " INSERT INTO [dbo].[DeletedRssItemsByUser]([RssItemId], [UserId]) " +
                             " VALUES (@newsId, @userId) ";
                db.Database.ExecuteSqlCommand(sql, parameterNewsId, parameterUserId);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public List<CountNewsOfCategory> GetCountsCategoriesOfRssChannels()
        {
            string sql = " SELECT * FROM [dbo].GetCountsCategoriesOfChannels() ";
            var countsList = db.Database.SqlQuery<CountNewsOfCategory>(sql).ToList();
            return countsList;
        }

        public List<CountNewsOfCategory> GetCountsCategoriesOfSubscribedRssChannels(string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректный идентификатор пользователя при запросе количества новостей для подписанных каналов");

                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT * FROM [dbo].GetCountsCategoriesOfSubscribedChannels(@userId) ";
                var countsList = db.Database.SqlQuery<CountNewsOfCategory>(sql, parameterUserId).ToList();
                return countsList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public List<CountNewsOfCategory> GetCountsCategoriesOfRssChannel(int channelId, string userId)
        {            
            try
            {
                if (channelId <= 0)
                    throw new Exception("Некорректный идентификатор канала при запросе количества новостей по категориям канала");
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректный идентификатор пользователя при запросе количества новостей по категориям канала");

                var parameterChannelId = new SqlParameter("@channelId", channelId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT * FROM [dbo].GetCountsCategoriesOfChannel(@channelId, @userId) ";
                List<CountNewsOfCategory> counts = db.Database.SqlQuery<CountNewsOfCategory>(sql, parameterChannelId, parameterUserId).ToList();
                return counts;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public List<RssItem> GetRssNewsByCategory(string category)
        {
            var parameterCategoryName = new SqlParameter("@categoryName", category);
            string sql = " SELECT RI.[Id], [RI].[Title], RI.[Link], RI.[Description], RI.[Date], RI.[ImageSrc], RI.[RssChannelId] " +
                         " FROM [dbo].[RssItems] AS RI, [RssCategories] AS RC, [dbo].[RssItemRssCategories] AS RIRC " +
                         " WHERE RI.[Id] = RIRC.[RssItem_Id] AND " +
                               " RC.[Id] = RIRC.[RssCategory_Id] AND " +
                               " RC.[Name] = @categoryName ";
            try
            {
                List<RssItem> items = db.Database.SqlQuery<RssItem>(sql, parameterCategoryName).ToList();
                return items;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public bool IsUserSubcribeOnRssChannel(int channelId)
        {
            var parameterChannelId = new SqlParameter("@channelId", channelId);
            string sql = " SELECT TOP(1) [RssChannelId] FROM [dbo].[UserRssChannels] " +
                         " WHERE [RssChannelId] = @channelId ";
            try
            {
                int? firstRecordId = db.Database.SqlQuery<int>(sql, parameterChannelId).FirstOrDefault();
                if (firstRecordId == null || firstRecordId == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
        }

        public void AddUserSubscriptionOnRssChannel(int channelId, string userId)
        {
            try
            {
                if (channelId <= 0)
                    throw new Exception("Некорректный идентификатор при добавлении RSS-канала в подписки пользователя");
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректное идентификатор пользователя при добавлении RSS-канала в подписки");

                var parameterChannelId = new SqlParameter("@channelId", channelId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " INSERT INTO [dbo].[UserRssChannels]([RssChannelId], [UserId]) " +
                             " VALUES (@channelId, @userId) ";
                db.Database.ExecuteSqlCommand(sql, parameterChannelId, parameterUserId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public void DeleteSubcriptionOnRssChannel(int channelId, string userId)
        {
            try
            {
                if (channelId <= 0)
                    throw new Exception("Некорректный идентификатор при удалении канала из подписок пользователя");
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректный идентификатор пользователя при удалении канала из подписок");

                var parameterChannelId = new SqlParameter("@channelId", channelId);
                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " DELETE FROM [dbo].[UserRssChannels] " +
                             " WHERE [RssChannelId] = @channelId AND " +
                                   " [UserId] = @userId ";
                db.Database.ExecuteSqlCommand(sql, parameterChannelId, parameterUserId);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        public List<ChannelNewsCount> GetSubscribedRssChannels(string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректный идентификатор пользователя при запросе каналов пользователя");

                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT MAX(RC.[Title]) AS [Title], COUNT(RC.[Id]) AS [Count], RC.[Id] " +
                             " FROM [dbo].[RssChannels] AS RC " +
                             " INNER JOIN [dbo].[UserRssChannels] AS URC ON RC.[Id] = URC.[RssChannelId] " +                            
                             " INNER JOIN [dbo].[RssItems] AS RI ON RI.[RssChannelId] = RC.[Id] " +
                             " WHERE URC.[UserId] = @userId " +
                             " GROUP BY RC.[Id] ";
                var channelNewsCounts = db.Database.SqlQuery<ChannelNewsCount>(sql, parameterUserId).ToList();
                return channelNewsCounts;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public RssItem GetLastRssItemOfChannel(int channelId)
        {
            SqlParameter parameterChannelId = new SqlParameter("@channelId", channelId);
            string sql = " SELECT TOP(1) * " +
                         " FROM [RssItems] " +
                         " WHERE [RssChannelId] = @channelId; ";
            try
            {
                RssItem lastRssItemInDb = db.Database.SqlQuery<RssItem>(sql, parameterChannelId).FirstOrDefault();
                return lastRssItemInDb;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return null;
            }
        }

        public List<RssItem> GetUserRssNews(string userId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    throw new Exception("Некорректный идентификатор пользователя при запросе списка новостей");

                var parameterUserId = new SqlParameter("@userId", userId);
                string sql = " SELECT * " +
                             " FROM [dbo].[RssItems] AS RI " +
                             " INNER JOIN [dbo].[UserRssChannels] AS URC ON URC.[RssChannelId] = RI.[RssChannelId] " +
                             " ORDER BY RI.[Date] DESC ";
                var news = db.Database.SqlQuery<RssItem>(sql, parameterUserId).ToList();
                return news;
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