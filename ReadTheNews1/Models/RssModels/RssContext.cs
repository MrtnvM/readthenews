using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ReadTheNews.Models.RssModels
{
    public class RssContext : DbContext
    {
        public RssContext()
            : base("DefaultConnection") {  }

        public DbSet<RssCategory> RssCategories { get; set; }
        public DbSet<RssChannel> RssChannels { get; set; }
        public DbSet<RssItem> RssItems { get; set; }
    }
}