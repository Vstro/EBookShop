using System;
using System.Collections.Generic;

namespace BookHaven.Models
{
    public class IndexViewModel
    {
        public List<Book> LastUpdatedBooks { get; set; }
        public List<Book> RecommendedBooks { get; set; }
        public List<Book> FoundBooks { get; set; }
        public String SearchQuery { get; set; }
    }
}
