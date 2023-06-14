using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHaven.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;

namespace BookHaven.Areas.Work.Pages.Chapters
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext dbContext;

        public CreateModel(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public Guid BookId { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Title")]
            public String Title { get; set; }

            [Required]
            [Display(Name = "Text")]
            public String Text { get; set; }
        }

        public void OnGet(Guid id)
        {
            BookId = id;
        }

        public async Task<ActionResult> OnPostAsync(Guid id)
        {
            if (ModelState.IsValid)
            {
                BookId = id;
                Chapter chapter = new Chapter
                {
                    Number = dbContext.Chapters.Where(ch => ch.BookId == BookId).Count() + 1,
                    Title = Input.Title,
                    Text = Input.Text,
                    Book = dbContext.Books.Find(BookId)
                };
                EntityEntry x = await dbContext.Chapters.AddAsync(chapter);
                dbContext.SaveChanges();
                return RedirectToPage("/Work/Books/Edit", dbContext.Books.Find(BookId));
            }
            return Page();
        }
    }
}