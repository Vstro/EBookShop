using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookHaven.Models
{
    public class User : IdentityUser
    {
        public DateTime RegistrationDate { get; set; }       
        public DateTime LastLoginDate { get; set; }
        public bool IsBlocked { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public bool IsCreator { get; set; } = false;
        public List<Book> Books { get; set; } = new List<Book>();
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Cart> Carts { get; set; } = new List<Cart>();
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}