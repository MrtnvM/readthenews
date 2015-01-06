using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadTheNews.Models.RssModels
{
    public class RssCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name="Название категории")]
        [MaxLength(15)]
        [Index(IsUnique=true)]
        public string Name { get; set; }

        public virtual ICollection<RssItem> RssItems { get; set; }

        public RssCategory()
        {
            RssItems = new List<RssItem>();
        }
    }
}