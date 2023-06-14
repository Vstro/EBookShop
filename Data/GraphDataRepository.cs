using BookHaven.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookHaven.Data
{
    public class GraphDataRepository
    {
        private IGraphClient graphClient;

        public GraphDataRepository(IGraphClient graphClient)
        {
            this.graphClient = graphClient;
        }

        public async Task<List<Guid>> GetSimilarBooksIds(Guid bookId, int amount)
        {
            return (await graphClient.Cypher.Match("(b1:BookNode $booknode1)-[s:SIMILAR]-(b2:BookNode)")
                .WithParam("booknode1", new BookNode() { Id = bookId })
                .Return((b2, s) => new
                {
                    BookId = b2.As<BookNode>().Id,
                    s.As<SimilarityEdge>().Similarity
                })
                .ResultsAsync)
                .OrderByDescending(a => a.Similarity)
                .Select(a => a.BookId)
                .Take(amount)
                .ToList();
        }

        public async Task<List<Guid>> GetSimilarBooksIds(List<Guid> booksIds, int amount)
        {
            var resultBooks = new List<(Guid, int)>();
            for (int i = 0; i < booksIds.Count; i++)
            {
                resultBooks.AddRange(
                    (await graphClient.Cypher.Match("(b1:BookNode $booknode1)-[s:SIMILAR]-(b2:BookNode)")
                    .WithParam("booknode1", new BookNode() { Id = booksIds[i] })
                    .Return((b2, s) => new
                    {
                        BookId = b2.As<BookNode>().Id,
                        s.As<SimilarityEdge>().Similarity
                    })
                    .ResultsAsync)
                    .Select(a => (a.BookId, a.Similarity)));
            }

            return resultBooks
                .GroupBy(b => b.Item1)
                .Select(g => g.Aggregate((a, b) => (a.Item1, a.Item2 + b.Item2)))
                .OrderByDescending(a => a.Item2)
                .Select(a => a.Item1)
                .Take(amount)
                .ToList();
        }

        public async Task CreateNode(Guid id)
        {
            await graphClient.Cypher.Create("(b:BookNode $booknode)")
                .WithParam("booknode", new BookNode() { Id = id })
                .ExecuteWithoutResultsAsync();
        }

        public async Task DeleteNode(Guid id)
        {
            await graphClient.Cypher.Delete("(b:BookNode $booknode)")
                .WithParam("booknode", new BookNode() { Id = id })
                .ExecuteWithoutResultsAsync();
        }

        public async Task UpdateSimilarityRelation(Guid bookId1, Guid bookId2, int similarity)
        {
            var edge = (await graphClient.Cypher.Match("(:BookNode $booknode1)-[s:SIMILAR]-(:BookNode $booknode2)")
                .WithParams(new Dictionary<string, object>() 
                    { { "booknode1", new BookNode() { Id = bookId1 } }, 
                      { "booknode2", new BookNode() { Id = bookId2 } } })
                .Return(s => s.As<SimilarityEdge>()).ResultsAsync).FirstOrDefault();

            similarity += edge?.Similarity ?? 0;

            await graphClient.Cypher.Match("(b1:BookNode $booknode1)")
                .WithParam("booknode1", new BookNode() { Id = bookId1 })
                .Match("(b2:BookNode $booknode2)")
                .WithParam("booknode2", new BookNode() { Id = bookId2 })
                .Merge("(b1)-[s:SIMILAR $similar]-(b2)")
                .WithParam("similar", new SimilarityEdge { Similarity = similarity })
                .ExecuteWithoutResultsAsync();
        }
    }
}
