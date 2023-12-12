using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(); // Return all books if no search term is provided.

            searchTerm = searchTerm.ToLower();

            // Query the database using EF methods and LINQ to filter based on the search term.
            var query = _entities
        .Include(book => book.Author)
        .Include(book => book.Genre)
        .Where(book =>
            book.Title.ToLower().Contains(searchTerm) ||
            book.Author.Name.ToLower().Contains(searchTerm) ||
            book.Genre.Name.ToLower().Contains(searchTerm));

            return await query.ToListAsync();
        }
    }
}