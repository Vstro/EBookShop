using System;

namespace BookHaven.Models
{
    public class Like
    {
        public Guid ChapterId { get; set; }
        public String UserId { get; set; }
        public Chapter Chapter { get; set; }
        public User User { get; set; }
    }
}
