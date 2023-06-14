using BookHaven.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookHaven.Services
{
    public interface IGraphService
    {
        Task<List<Guid>> GetRecommendedBooksIds(string userId, int amount);

        Task<List<Guid>> GetSimilarBooksIds(Guid bookId, int amount);

        void AddBook(Guid id);

        void DeleteBook(Guid id);

        void RecordActionInterest(string userId, Guid bookId, ActionInterest action);
    }
}
