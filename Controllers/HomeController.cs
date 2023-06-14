using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using BookHaven.Models;
using BookHaven.Data;
using System;
using Microsoft.EntityFrameworkCore;
using BookHaven.Services;
using Braintree;

namespace BookHaven.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<User> userManager;
        private SignInManager<User> signInManager;
        private ApplicationDbContext dbContext;
        private IBraintreeService braintreeService;
        private IEmailService emailService;
        private IGraphService graphService;


        public HomeController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext dbContext,
            IBraintreeService braintreeService,
            IEmailService emailService,
            IGraphService graphService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
            this.braintreeService = braintreeService;
            this.emailService = emailService;
            this.graphService = graphService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null && user.IsBlocked)
            {
                return LocalRedirect("/Identity/Account/Lockout");
            }
            if (user != null && !user.EmailConfirmed)
            {
                return LocalRedirect("/Identity/Account/NotConfirmedEmail");
            }
            ViewData["scroll"] = false;
            IndexViewModel indexViewModel = new IndexViewModel
            {
                LastUpdatedBooks = dbContext.Books
                    .OrderBy(f => DateTime.Now.Subtract(f.LastUpdate))
                    .Take(10)
                    .Include(b => b.User)
                    .ToList(),
                FoundBooks = null
            };

            if (user != null)
            {
                var recommendedBooksIds = await graphService.GetRecommendedBooksIds(user.Id, 10);
                indexViewModel.RecommendedBooks = dbContext.Books
                    .Where(b => recommendedBooksIds.Contains(b.Id))
                    .Include(b => b.User)
                    .ToList();
            }

            return View(indexViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery)
        {
            List<Book> foundBooks = new List<Book>();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var keyWords = searchQuery.Split(" ");
                foundBooks = dbContext.Books.Include(b => b.BookTags)
                                 .ThenInclude(bt => bt.Tag)
                                 .Include(b => b.User).ToList();
                foundBooks = foundBooks.Where(b => keyWords.Any(
                                     kw => b.Title.Contains(kw, StringComparison.OrdinalIgnoreCase)
                                     || b.Author.Contains(kw, StringComparison.OrdinalIgnoreCase)
                                     || b.Genre.Contains(kw, StringComparison.OrdinalIgnoreCase)
                                     || b.BookTags.Any(bt => bt.Tag.Value.Contains(kw, StringComparison.OrdinalIgnoreCase))))
                                 .ToList();
            }
            ViewData["scroll"] = true;
            IndexViewModel indexViewModel = new IndexViewModel
            {
                LastUpdatedBooks = dbContext.Books
                    .OrderBy(f => DateTime.Now.Subtract(f.LastUpdate))
                    .Take(10)
                    .Include(b => b.User)
                    .ToList(),
                FoundBooks = foundBooks,
                SearchQuery = searchQuery
            };

            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var recommendedBooksIds = await graphService.GetRecommendedBooksIds(user.Id, 10);
                indexViewModel.RecommendedBooks = dbContext.Books
                    .Where(b => recommendedBooksIds.Contains(b.Id))
                    .Include(b => b.User)
                    .ToList();
            }

            return View(indexViewModel);
        }

        public async Task<IActionResult> ReadingBook(Guid id, bool scroll = false)
        {
            Book currentBook = dbContext.Books.Include(b => b.BookTags)
                             .ThenInclude(bt => bt.Tag)
                             .Include(b => b.Reviews)
                             .ThenInclude(r => r.User)
                             .FirstOrDefault(f => f.Id.Equals(id));
            dbContext.Entry(currentBook).Collection(f => f.Chapters).Load();
            var reviews = new List<ReviewViewModel>();
            float rating = 0;
            if (currentBook.Reviews != null && currentBook.Reviews.Count > 0)
            {
                reviews = currentBook.Reviews
                .Select(r => new ReviewViewModel()
                {
                    UserName = r.User.UserName,
                    Rate = r.Rate,
                    Text = r.Text
                })
                .ToList();
                rating = (float)(reviews.Sum(r => r.Rate) * 1.0 / reviews.Count);
            }
            ViewData["scroll"] = scroll;

            bool isInCart = false;
            bool isPurchased = false;
            bool isReviewed = false;
            User user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                graphService.RecordActionInterest(user.Id, id, ActionInterest.View);

                var carts = dbContext.Carts.Where(c => c.UserId == user.Id)
                    .Include(c => c.CartBooks)
                    .ToList();
                for (int i = 0; i < carts.Count; i++)
                {
                    for (int j = 0; j < carts[i].CartBooks.Count; j++)
                    {
                        if (carts[i].CartBooks[j].BookId == id)
                        {
                            isInCart = carts[i].IsActive;
                            isPurchased = !carts[i].IsActive;
                            break;
                        }
                    }
                }
                if (currentBook.Reviews != null && currentBook.Reviews.Count > 0)
                {
                    isReviewed = currentBook.Reviews.Any(r => r.UserId == user.Id);
                }
            }

            var similarBooksIds = await graphService.GetSimilarBooksIds(id, 5);
            var similarBooks = dbContext.Books
                .Where(b => similarBooksIds.Contains(b.Id))
                .Include(b => b.User)
                .ToList();

            return View(new ReadingBookViewModel()
            {
                Book = currentBook,
                Rating = rating,
                IsInCart = isInCart,
                IsPurchased = isPurchased,
                IsReviewed = isReviewed,
                Reviews = reviews,
                SimilarBooks = similarBooks
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(Guid bookId, int rate, string reviewText)
        {
            User user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                graphService.RecordActionInterest(user.Id, bookId, ActionInterest.Review);

                var review = new Review()
                {
                    UserId = user.Id,
                    Rate = rate,
                    Text = reviewText,
                    BookId = bookId
                };
                dbContext.Reviews.Add(review);
                dbContext.SaveChanges();
            }
            return RedirectToAction("ReadingBook", new { id = bookId, scroll = true });
        }

        public IActionResult DeleteBook(Guid id, string userId)
        {
            var deletingBook = dbContext.Books.Where(b => b.Id == id).FirstOrDefault();
            dbContext.Books.Remove(deletingBook);
            dbContext.SaveChanges();
            graphService.DeleteBook(id);

            return RedirectToAction("TheAuthorBooks", new { id = userId });
        }

        [HttpGet]
        public IActionResult RemoveBook(Guid id, Guid cartId)
        {
            var cart = dbContext.Carts.Where(c => c.Id == cartId)
                    .Include(c => c.CartBooks)
                    .ToList().SingleOrDefault();
            cart.CartBooks.RemoveAll(cb => cb.BookId == id);
            dbContext.Carts.Update(cart);
            dbContext.SaveChanges();
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> AddToCart(Guid id)
        {
            User user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var cart = dbContext.Carts.Where(c => c.IsActive && c.UserId == user.Id)
                    .Include(c => c.CartBooks)
                    .ThenInclude(cb => cb.Book).ToList()
                    .FirstOrDefault();
                var book = dbContext.Books.Find(id);
                // Create empty cart if user didn't add anything and didn't created one yet
                if (cart == null)
                {
                    cart = new Cart()
                    {
                        UserId = user.Id,
                        IsActive = true
                    };
                    dbContext.Carts.Add(cart);
                    dbContext.SaveChanges();
                }
                cart.CartBooks.Add(new CartBook()
                {
                    CartId = cart.Id,
                    Cart = cart,
                    BookId = id,
                    Book = book
                });
                dbContext.Carts.Update(cart);
                dbContext.SaveChanges();
            }
            return RedirectToAction("ReadingBook", new { id });
        }

        public async Task<IActionResult> ResendBook(Guid id)
        {
            User user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var bookFiles = dbContext.Books
                    .Where(b => b.Id == id)
                    .Include(b => b.Base64File)
                    .Select(b => b.Base64File)
                    .DefaultIfEmpty()
                    .ToArray();
                if (bookFiles.Length == 0 || bookFiles[0] == null)
                {
                    bookFiles = null;
                }
                await emailService.SendEmailAsync(
                    user.Email,
                    "Resend purchased book",
                    $"Thank you, {user.UserName}, once again for your purchase! You can find your book attached.",
                    bookFiles);
            }
            return RedirectToAction("ReadingBook", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> MakeOrder(CartViewModel model)
        {
            User user = await userManager.GetUserAsync(User);
            dbContext.Books.Where(b => model.Books.Any(cb => cb.Id == b.Id)).Load();

            var gateway = braintreeService.GetGateway();
            var request = new TransactionRequest
            {
                Amount = model.Books.Select(b => b.Cost).Sum(),
                PaymentMethodNonce = model.Nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            Result<Transaction> result = gateway.Transaction.Sale(request);

            if (result.IsSuccess())
            {
                foreach (Guid bookId in model.Books.Select(b => b.Id))
                {
                    graphService.RecordActionInterest(user.Id, bookId, ActionInterest.Buy);
                }

                // Send purchased books to the customer email
                var purchasedBookFiles = dbContext.Books.Where(b => model.Books.Any(pb => pb.Id == b.Id))
                    .Include(b => b.Base64File)
                    .Select(b => b.Base64File)
                    .Where(b => b != null)
                    .DefaultIfEmpty()
                    .ToArray();
                if (purchasedBookFiles.Length == 0 || purchasedBookFiles[0] == null)
                {
                    purchasedBookFiles = null;
                }
                await emailService.SendEmailAsync(
                    user.Email, 
                    "Purchased books",
                    $"Thank you, {user.UserName}, for your purchase! You can find all your books attached. Also you always can resend a purchased book to your email again with the appropriate button on the purchased book's page.",
                    purchasedBookFiles);

                // Inactivate used cart
                var cart = dbContext.Carts.Where(c => c.Id == model.CartId).FirstOrDefault();
                cart.IsActive = false;
                dbContext.Carts.Update(cart);
                dbContext.SaveChanges();

                return View("OrderSuccess");
            }
            else
            {
                return View("OrderFailure");
            }
        }

        public async Task<IActionResult> Administration()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null && user.IsBlocked)
            {
                return LocalRedirect("/Identity/Account/Lockout");
            }
            if (user != null && !user.EmailConfirmed)
            {
                return LocalRedirect("/Identity/Account/NotConfirmedEmail");
            }
            return View(userManager.Users.ToList());              
        }

        public async Task<IActionResult> TheAuthorBooks(String id = null)
        {
            User user;
            if (id == null)
            {
                user = await userManager.GetUserAsync(User);
            }
            else
            {
                user = await userManager.FindByIdAsync(id);
            }
            if (user != null)
            {
                if (user.IsBlocked)
                {
                    return LocalRedirect("/Identity/Account/Lockout");
                }
                if (!user.EmailConfirmed)
                {
                    return LocalRedirect("/Identity/Account/NotConfirmedEmail");
                }
                dbContext.Books.Where(f => f.UserId == user.Id).Load();
                return View(new TheAuthorBooksViewModel {
                    Books = user.Books,
                    AuthorName = user.UserName,
                    AuthorId =  user.Id});
            }
            if (id == null)
                return RedirectToAction("Index");
            return RedirectToAction("Administration");
        }

        public async Task<IActionResult> Cart()
        {
            User user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                if (user.IsBlocked)
                {
                    return LocalRedirect("/Identity/Account/Lockout");
                }
                if (!user.EmailConfirmed)
                {
                    return LocalRedirect("/Identity/Account/NotConfirmedEmail");
                }

                // Save client braintree token in ViewBag
                var gateway = braintreeService.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;

                var cart = dbContext.Carts.Where(c => c.IsActive && c.UserId == user.Id)
                    .Include(c => c.CartBooks)
                    .ThenInclude(cb => cb.Book).ToList()
                    .FirstOrDefault();
                var books = new List<Book>();
                
                if (cart != null)
                {
                    for (int i = 0; i < cart.CartBooks.Count; i++)
                    {
                        books.Add(cart.CartBooks[i].Book);
                    }
                }
                else // Create empty cart if user didn't add anything and didn't created one yet
                {
                    cart = new Cart()
                    {
                        UserId = user.Id,
                        IsActive = true
                    };
                    dbContext.Carts.Add(cart);
                    dbContext.SaveChanges();
                }
                return View(new CartViewModel
                {
                    Books = books,
                    CartId = cart.Id,
                    Nonce = ""
                });
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ReadingChapterAsync(Guid id)
        {
            Chapter currentChapter = dbContext.Chapters.Find(id);
            Chapter prevChapter;
            List<Chapter> prevChapters = dbContext.Chapters.Where(ch => ch.Number == currentChapter.Number - 1).ToList();
            if (prevChapters.Count != 0)
            {
                prevChapters = prevChapters.Where(ch => ch.BookId == currentChapter.BookId).ToList();
                if (prevChapters.Count != 0)
                {
                    prevChapter = prevChapters[0];
                }
                else
                {
                    prevChapter = null;
                }
            }
            else
            {
                prevChapter = null;
            }
            Chapter nextChapter;
            List<Chapter> nextChapters = dbContext.Chapters.Where(ch => ch.Number == currentChapter.Number + 1).ToList();
            if (nextChapters.Count != 0)
            {
                nextChapters = nextChapters.Where(ch => ch.BookId == currentChapter.BookId).ToList();
                if (nextChapters.Count != 0)
                {
                    nextChapter = nextChapters[0];
                }
                else
                {
                    nextChapter = null;
                }
            }
            else
            {
                nextChapter = null;
            }
            List<User> likedUsers = new List<User>();
            List<User> users = dbContext.Users.Include(u => u.Likes).ThenInclude(l => l.Chapter).ToList();
            foreach(User user in users)
            {
                if (user.Likes.FindAll(l => l.ChapterId == id).ToList().Count > 0)
                {
                    likedUsers.Add(user);
                }
            }
            ReadingChapterViewModel viewModel = new ReadingChapterViewModel
            {
                CurrentChapter = currentChapter,
                PrevChapterId = prevChapter == null ? Guid.Empty : prevChapter.Id,
                NextChapterId = nextChapter == null ? Guid.Empty : nextChapter.Id,
                LikedUsersIds = likedUsers.Select(u => u.Id).ToList()
            };

            var signedUser = await userManager.GetUserAsync(User);
            if (signedUser != null)
            {
                graphService.RecordActionInterest(signedUser.Id, currentChapter.BookId, ActionInterest.Read);
            }

            return View(viewModel);
        }

        public IActionResult Like(Guid chapterid, String userid)
        {
            User likedUser = dbContext.Users.Include(u => u.Likes).Where(u => u.Id == userid).ToList()[0];
            likedUser.Likes.Add(new Like { User = likedUser, Chapter = dbContext.Chapters.Find(chapterid) });
            dbContext.Users.Update(likedUser);
            Chapter likedChapter = dbContext.Chapters.Find(chapterid);
            likedChapter.LikesAmount++;
            dbContext.Chapters.Update(likedChapter);
            dbContext.SaveChanges();
            return RedirectToAction("ReadingChapter", new { id = chapterid });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(String userId, String code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return LocalRedirect("/Identity/Account/ConfirmedEmail");
            }
            else
                return View("Error");
        }

        public async Task<IActionResult> CheckCurrentUserStatus()
        {
            User currentUser = await userManager.GetUserAsync(User);
            if ((currentUser == null) || (currentUser.IsBlocked))
            {
                await signInManager.SignOutAsync();
                return Unauthorized();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> BlockSelected(List<User> users)
        {
            if (await CheckCurrentUserStatus() is OkResult)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].IsSelected)
                    {
                        await ChangeUserStatus(users[i].Id, true);
                    }
                }
            }
            return RedirectToAction("Administration");
        }

        [HttpGet]
        public async Task<IActionResult> UnblockSelected(List<User> users)
        {
            if (await CheckCurrentUserStatus() is OkResult)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].IsSelected)
                    {
                        await ChangeUserStatus(users[i].Id, false);
                    }
                }
            }
            return RedirectToAction("Administration");
        }

        [HttpGet]
        public async Task<IActionResult> MakeSelectedCreators(List<User> users)
        {
            if (await CheckCurrentUserStatus() is OkResult)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].IsSelected)
                    {
                        await ChangeUserRole(users[i].Id, true);
                    }
                }
            }
            return RedirectToAction("Administration");
        }

        [HttpGet]
        public async Task<IActionResult> MakeSelectedNoncreators(List<User> users)
        {
            if (await CheckCurrentUserStatus() is OkResult)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].IsSelected)
                    {
                        await ChangeUserRole(users[i].Id, false);
                    }
                }
            }
            return RedirectToAction("Administration");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteSelected(List<User> users)
        {
            if (await CheckCurrentUserStatus() is OkResult)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    if (users[i].IsSelected)
                    {
                        await DeleteUser(users[i].Id);
                    }
                }
            }
            return RedirectToAction("Administration");
        }

        public async Task ChangeUserStatus(String userId, bool isBlocked)
        {
            User user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsBlocked = isBlocked;
                await userManager.UpdateAsync(user);
            }
        }

        public async Task ChangeUserRole(String userId, bool isCreator)
        {
            User user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsCreator = isCreator;
                await userManager.UpdateAsync(user);
            }
        }

        public async Task DeleteUser(String id)
        {
            User user = await userManager.FindByIdAsync(id);
            await userManager.DeleteAsync(user);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
