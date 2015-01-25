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
        public DbSet<DeletedRssItemsByUser> DeletedRssItemsByUser { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeletedRssItemsByUser>().HasKey(t => new { t.RssItemId, t.UserId });

            base.OnModelCreating(modelBuilder);
        }
    }
}