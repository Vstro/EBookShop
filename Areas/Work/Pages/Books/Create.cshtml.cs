using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHaven.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using BookHaven.Services;

namespace BookHaven.Areas.Work.Pages.Books
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext dbContext;
        private IGraphService graphService;

        public CreateModel(ApplicationDbContext dbContext,
            IGraphService graphService)
        {
            this.dbContext = dbContext;
            this.graphService = graphService;
        }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }
        public String UserId { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Title")]
            public String Title { get; set; }

            [Required]
            [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
            [Display(Name = "Author")]
            public String Author { get; set; }

            [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 0)]
            [Display(Name = "Summary")]
            public String Summary { get; set; }

            [Required]
            [Display(Name = "Genre")]
            public String Genre { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            [Display(Name = "Price")]
            public double Price { get; set; } = 0;

            [Display(Name = "Upload your book file")]
            public IFormFile File { get; set; }

            [Display(Name = "Tags separated by space")]
            public String Tags { get; set; }
        }

        public void OnGet(String id)
        {
            ModelState.Clear();
            UserId = id;
        }

        public async Task<ActionResult> OnPostAsync(String id)
        {
            UserId = id;
            if (ModelState.IsValid)
            {
                Base64File base64File = null;
                if (Input.File != null)
                {
                    var ms = new MemoryStream();
                    Input.File.CopyTo(ms);
                    base64File = new Base64File()
                    {
                        FileName = Input.File.FileName,
                        Data = Convert.ToBase64String(ms.ToArray())
                    };
                    ms.Close();
                }              

                Book book = new Book
                {
                    Title = Input.Title,
                    Author = Input.Author,
                    Summary = Input.Summary,
                    Genre = Input.Genre,
                    Cost = (decimal)Input.Price,
                    Base64File = base64File,
                    UserId = UserId
                };

                if (!string.IsNullOrEmpty(Input.Tags))
                {
                    String[] tagValues = Input.Tags.Split(' ');
                    Tag[] tags = new Tag[tagValues.Length];
                    for (int i = 0; i < tags.Length; i++)
                    {
                        tags[i] = new Tag { Value = tagValues[i] };
                    }
                    dbContext.Tags.AddRange(tags);
                    foreach (Tag tag in tags)
                    {
                        book.BookTags.Add(new BookTag { Tag = tag, Book = book });
                    }
                }
                
                await dbContext.Books.AddAsync(book);
                dbContext.SaveChanges();
                if (book.Id != Guid.Empty) graphService.AddBook(book.Id);
                return RedirectToPage("./Edit", book);
            }
            return Page();
        }
    }
}