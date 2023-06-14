using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class Chapter
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public long LikesAmount { get; set; }
        public String Title { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public String Text { get; set; }
        public String ImageLink { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}
