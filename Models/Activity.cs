using System;

namespace BookHaven.Models
{
    public class Activity
    {
        public Guid Id { get; set; }
        public String ActionInterest { get; set; }
        public Guid BookId { get; set; }
        public String UserId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
    }
}
