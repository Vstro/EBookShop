using System;
using System.Collections.Generic;

namespace BookHaven.Models
{
    public class CartViewModel
    {
        public List<Book> Books { get; set; }
        public Guid CartId { get; set; }
        public string Nonce { get; set; }
    }
}
