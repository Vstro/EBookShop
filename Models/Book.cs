using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHaven.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public String Title { get; set; }
        public String Author { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public String Summary { get; set; }
        public String Genre { get; set; }
        [Column(TypeName = "money")]
        public decimal Cost { get; set; }
        public List<BookTag> BookTags { get; set; } = new List<BookTag>();
        public List<CartBook> CartBooks { get; set; } = new List<CartBook>();
        [Column(TypeName = "bigint")]
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public List<Chapter> Chapters { get; set; } = new List<Chapter>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public Base64File Base64File { get; set; }
        public String UserId { get; set; }
        public User User { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}
