using WebApplication2.Models;

namespace WebApplication2
{
public interface IBookRepository : IRepository<Book>
{
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
}

}
