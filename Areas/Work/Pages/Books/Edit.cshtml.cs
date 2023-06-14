using System;
using System.ComponentModel.DataAnnotations;
using BookHaven.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHaven.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Http.Internal;

namespace BookHaven.Areas.Work.Pages.Books
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
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public List<Chapter> Chapters {get; set; }

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

        [HttpGet]
        public void OnGet(Book book)
        {
            ModelState.Clear();
            Book = book;
            if (book != null)
            {
                book = dbContext.Books.Include(f => f.BookTags).ThenInclude(ft => ft.Tag).Where(f => f.Id == book.Id).ToList()[0];
                Input.Title = book.Title;
                Input.Summary = book.Summary;
                Input.Genre = book.Genre;
                Input.Price = Convert.ToDouble(book.Cost);
                Input.Author = book.Author;

                var base64file = dbContext.Base64Files
                .Where(b => b.BookId == book.Id)
                .FirstOrDefault();
                if (base64file != null)
                {
                    byte[] bytes = Convert.FromBase64String(base64file.Data);
                    MemoryStream stream = new MemoryStream(bytes);
                    IFormFile file = new FormFile(stream, 0, bytes.Length, base64file.FileName, base64file.FileName);
                    Input.File = file;
                }

                if (book.BookTags.Count > 0)
                {
                    Input.Tags = book.BookTags.Select(ft => ft.Tag.Value).Aggregate((s1, s2) => s1 + " " + s2);
                }
                Chapters = dbContext.Chapters.Where(ch => ch.Book == book).ToList();
            }
        }

        [HttpPost]
        public ActionResult OnPost(Book book)
        {
            if (ModelState.IsValid)
            {
                Book updatingBook = dbContext.Books.Include(b => b.BookTags)
                    .ThenInclude(bt => bt.Tag)
                    .Include(b => b.Base64File)
                    .Where(b => b.Id == book.Id).FirstOrDefault();
                updatingBook.Title = Input.Title;
                updatingBook.Author = Input.Author;
                updatingBook.Summary = Input.Summary;
                updatingBook.Genre = Input.Genre;
                updatingBook.LastUpdate = DateTime.Now;
                updatingBook.Cost = (decimal)Input.Price;

                if (Input.File != null)
                {
                    var ms = new MemoryStream();
                    Input.File.CopyTo(ms);
                    var base64File = new Base64File()
                    {
                        FileName = Input.File.FileName,
                        Data = Convert.ToBase64String(ms.ToArray())
                    };
                    ms.Close();
                    updatingBook.Base64File = base64File;
                    dbContext.Base64Files.Add(base64File);
                }
                else if (updatingBook.Base64File != null)
                {
                    byte[] bytes = Convert.FromBase64String(updatingBook.Base64File.Data);
                    MemoryStream stream = new MemoryStream(bytes);
                    IFormFile file = new FormFile(stream, 0, bytes.Length, updatingBook.Base64File.FileName, updatingBook.Base64File.FileName);
                    Input.File = file;
                }

                if (!string.IsNullOrEmpty(Input.Tags))
                {
                    String[] tagValues = Input.Tags.Split(' ');
                    Tag[] newTags = new Tag[tagValues.Length];
                    for (int i = 0; i < newTags.Length; i++)
                    {
                        newTags[i] = new Tag { Value = tagValues[i] };
                    }
                    Tag[] oldTags = new Tag[updatingBook.BookTags.Count];
                    for (int i = 0; i < updatingBook.BookTags.Count; i++)
                    {
                        oldTags[i] = updatingBook.BookTags[i].Tag;
                    }
                    updatingBook.BookTags.Clear();
                    dbContext.Tags.RemoveRange(oldTags);
                    dbContext.Tags.AddRange(newTags);
                    foreach (Tag tag in newTags)
                    {
                        updatingBook.BookTags.Add(new BookTag { Tag = tag, Book = updatingBook });
                    }
                }

                dbContext.Books.Update(updatingBook);
                dbContext.SaveChanges();
            }
            Book = book;
            Chapters = dbContext.Chapters.Where(ch => ch.Book == book).ToList();
            return Page();
        }
    }
}