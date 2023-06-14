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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext dbContext;

        public EditModel(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }

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

        [HttpGet]
        public void OnGet(Guid id)
        {
            ModelState.Clear();
            Chapter chapter = dbContext.Chapters.Find(id);
            if (chapter != null)
            {
                Input.Title = chapter.Title;
                Input.Text = chapter.Text;
            }
        }

        [HttpPost]
        public ActionResult OnPostAsync(Guid id)
        {
            if (ModelState.IsValid)
            {
                Chapter chapter = dbContext.Chapters.Find(id);
                chapter.Title = Input.Title;
                chapter.Text = Input.Text;
                dbContext.Chapters.Update(chapter);
                dbContext.SaveChanges();
                return RedirectToPage("/Work/Books/Edit", dbContext.Books.Find(chapter.BookId));
            }
            return Page();
        }
    }
}