using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using ReadTheNews.Helpers;
using ReadTheNews.Models;

namespace ReadTheNews.Modules
{
    public class NewsRefreshingModule : IHttpModule
    {
        private static Timer timer;
        private long interval = 3600; // 1 hour
        private static object synclock = new object();

        public void Init(HttpApplication app)
        {
            timer = new Timer(new TimerCallback(RefreshNews), null, 0, interval);
        }

        public void RefreshNews(object obj)
        {
            lock (synclock)
            {
                using (var db = new RssContext())
                {
                    var channels = db.RssChannels.ToList();
                    foreach (var channel in channels)
                    {
                        var processor = RssProcessor.GetRssProcessor(channel);
                        processor.GetLatestNews("ba44f20e-3580-4307-b7dc-cf585a9e4a16");
                        processor.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {

        }
    }
}