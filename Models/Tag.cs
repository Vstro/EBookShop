using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public String Value { get; set; }
        public List<BookTag> BookTags { get; set; } = new List<BookTag>();
    }
}
