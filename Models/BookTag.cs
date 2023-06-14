using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class BookTag
    {
        public Guid BookId { get; set; }
        public Guid TagId { get; set; }
        public Book Book { get; set; }
        public Tag Tag { get; set; }
    }
}
