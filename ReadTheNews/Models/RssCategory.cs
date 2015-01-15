using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadTheNews.Models
{
    public class RssCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name="Название категории")]
        [MaxLength(40)]
        public string Name { get; set; }

        public virtual ICollection<RssItem> RssItems { get; set; }

        public RssCategory()
        {
            RssItems = new List<RssItem>();
        }
    }
}