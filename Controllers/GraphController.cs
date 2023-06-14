using BookHaven.Data;
using BookHaven.Models;
using BookHaven.Services;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookHaven.Controllers
{
    public class GraphController : Controller
    {
        private GraphService graphService;

        public GraphController(
            ApplicationDbContext dbContext, 
            IGraphClient graphClient)
        {
            graphService = new GraphService(dbContext, graphClient);
        }

        [HttpGet]
        public async Task<List<Guid>> GetRecommendedBooksIds(string userId, int amount = 5)
        {
            return await graphService.GetRecommendedBooksIds(userId, amount);
        }

        [HttpGet]
        public async Task<List<Guid>> GetSimilarBooksIds(Guid bookId, int amount = 5)
        {
            return await graphService.GetSimilarBooksIds(bookId, amount);
        }

        [HttpPost]
        public void AddBook(Guid id)
        {
            graphService.AddBook(id);
        }

        [HttpPost]
        public void DeleteBook(Guid id)
        {
            graphService.DeleteBook(id);
        }

        [HttpPost]
        public void RecordActionInterest(string userId, Guid bookId, ActionInterest action)
        {
            graphService.RecordActionInterest(userId, bookId, action);
        }
    }
}
