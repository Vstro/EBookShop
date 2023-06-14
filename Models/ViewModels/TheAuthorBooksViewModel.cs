using System;
using System.Collections.Generic;

namespace BookHaven.Models
{
    public class TheAuthorBooksViewModel
    {
        public List<Book> Books { get; set; }
        public String AuthorName { get; set; }
        public String AuthorId { get; set; }
    }
}
