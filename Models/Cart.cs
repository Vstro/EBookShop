using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Models
{
    public class Cart
    {
        public Guid Id { get; set; }
        [Column(TypeName = "money")]
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
        public String UserId { get; set; }
        public User User { get; set; }
        public List<CartBook> CartBooks { get; set; } = new List<CartBook>();
    }
}
