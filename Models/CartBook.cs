using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class CartBook
    {
        public Guid BookId { get; set; }
        public Guid CartId { get; set; }
        public Book Book { get; set; }
        public Cart Cart { get; set; }
    }
}
