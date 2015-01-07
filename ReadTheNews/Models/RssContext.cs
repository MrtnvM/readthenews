using System.Data.Entity;

namespace ReadTheNews.Models
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