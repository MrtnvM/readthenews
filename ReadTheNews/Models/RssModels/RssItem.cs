using System;
using System.Collections.Generic;
using System.Xml;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadTheNews.Models.RssModels
{
    public class RssItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name="Заголовок")]
        [MaxLength(120)]
        public string Title { get; set; }

        [Required]
        [Display(Name="Ссылка")]
        [MaxLength(150)]
        public string Link { get; set; }

        [Required]
        [Display(Name="Описание")]
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name="Дата публикации")]
        public DateTime Date { get; set; }

        [Display(Name="Адрес изображения")]
        [MaxLength(150)]
        public string ImageSrc { get; set; }

        public int RssChannelId { get; set; }
        public virtual RssChannel RssChannel { get; set; }

        public ICollection<RssCategory> Categories { get; set; }

        public RssItem()
        {
            Categories = new List<RssCategory>();
        }
    }
}
