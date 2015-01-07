using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace ReadTheNews.Models.RssModels
{
    public class RssChannel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name="Заголовок")]
        [MaxLength(120)]
        public string Title { get; set; }

        [Display(Name="Язык канала")]
        [MaxLength(4)]
        public string Language { get; set; }

        [Required]
        [Display(Name = "Ссылка")]
        [MaxLength(150)]
        public string Link { get; set; }

        [Required]
        [Display(Name = "Описание")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Display(Name = "Адрес изображения")]
        [MaxLength(150)]
        public string ImageSrc { get; set; }

        [Display(Name="Дата публикации")]
        [DataType(DataType.DateTime)]
        public DateTime PubDate { get; set; }

        public virtual ICollection<RssItem> RssItems { get; set; }

        public RssChannel()
        {
            RssItems = new List<RssItem>();
        }

        public async void GetLatestNewsAsync()
        {
            RssProcessor processor = await RssProcessor.GetRssProcessorAsync(this);

            DateTime latestUpdate = new DateTime();
            if (processor.Channel.LastUpdatedTime.DateTime != new DateTime())
                latestUpdate = processor.Channel.LastUpdatedTime.DateTime;
            else
                latestUpdate = processor.Channel.Items.First() != null ? 
                    processor.Channel.Items.First().PublishDate.DateTime : new DateTime();

            if (this.PubDate == latestUpdate)
                return;

            var db = new RssContext();

            db.RssItems.RemoveRange(this.RssItems);
            var rssItems = await processor.GetRssItemListAsync();
            this.RssItems = rssItems;
            db.Entry<RssChannel>(this).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }
    }
}
