using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookHaven.Models;

namespace BookHaven.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BookTag> BookTags { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<CartBook> CartBooks { get; set; }
        public DbSet<Base64File> Base64Files { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.UserId, l.ChapterId });
            modelBuilder.Entity<BookTag>()
                .HasKey(f => new { f.BookId, f.TagId });
            modelBuilder.Entity<CartBook>()
                .HasKey(f => new { f.BookId, f.CartId });
            modelBuilder.Entity<Book>()
                .HasOne(p => p.User)
                .WithMany(t => t.Books)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Like>()
                .HasOne(r => r.User)
                .WithMany(u => u.Likes)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.User)
                .WithMany(u => u.Activities)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Book)
                .WithMany(b => b.Activities)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Cart>()
               .HasOne(c => c.User)
               .WithMany(u => u.Carts)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
