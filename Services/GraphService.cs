using BookHaven.Data;
using BookHaven.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Services
{
    public class GraphService : IGraphService
    {
        private GraphDataRepository graphDataRepository;
        private ApplicationDbContext dbContext;

        public GraphService(ApplicationDbContext dbContext, 
            IGraphClient graphClient)
        {
            this.dbContext = dbContext;
            graphDataRepository = new GraphDataRepository(graphClient);
        }

        public async Task<List<Guid>> GetRecommendedBooksIds(string userId, int amount)
        {
            // Find all the books the user was interested in
            var relatedByUserActivityBooks = dbContext.Activities.Where(a => a.UserId == userId)
                .Select(a => a.BookId)
                .Distinct()
                .ToList();

            return await graphDataRepository.GetSimilarBooksIds(relatedByUserActivityBooks, amount);
        }

        public async Task<List<Guid>> GetSimilarBooksIds(Guid bookId, int amount)
        {
            return await graphDataRepository.GetSimilarBooksIds(bookId, amount);
        }

        public void AddBook(Guid id)
        {
            _ = graphDataRepository.CreateNode(id);
        }

        public void DeleteBook(Guid id)
        {
            _ = graphDataRepository.DeleteNode(id);
        }

        public void RecordActionInterest(string userId, Guid bookId, ActionInterest action)
        {
            // Prevent multiple recording of the same (repeating) activity
            var sameActivity = dbContext.Activities.Where(a => a.UserId == userId && a.BookId == bookId && a.ActionInterest.Equals(action))
                .FirstOrDefault();
            if (sameActivity != null) return;

            // Find all the books the user was interested in
            var relatedByUserActivityBooks = dbContext.Activities.Where(a => a.UserId == userId)
                .Select(a => a.BookId)
                .Distinct()
                .ToList();

            // Save new activity record
            var activity = new Activity()
            {
                ActionInterest = action.ToString(),
                BookId = bookId,
                UserId = userId
            };
            dbContext.Activities.Add(activity);
            dbContext.SaveChanges();

            // Connect (or update existing relations) new activity book with the previous ones
            for (int i = 0; i < relatedByUserActivityBooks.Count; i++)
            {
                _ = graphDataRepository.UpdateSimilarityRelation(bookId, relatedByUserActivityBooks[i], (int)action);
            }
        }
    }
}
