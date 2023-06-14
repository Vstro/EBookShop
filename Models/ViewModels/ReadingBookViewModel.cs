using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class ReadingBookViewModel
    {
        public Book Book { get; set; }
        public float Rating { get; set; }
        public bool IsInCart { get; set; }
        public bool IsPurchased { get; set; }
        public bool IsReviewed { get; set; }
        public List<ReviewViewModel> Reviews { get; set; }
        public List<Book> SimilarBooks { get; set; }
    }
}
