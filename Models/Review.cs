using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public int Rate { get; set; }
        public String Text { get; set; }
        public Guid BookId { get; set; }
        public String UserId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
    }
}
