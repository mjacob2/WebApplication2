using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2;
using WebApplication2.Data;
using WebApplication2.Models;

namespace Tests
{
    public class UnitTest1
    {
        private ApplicationDbContext CreateDbContext(string dbName)
        {
            // Create a fresh service provider, and therefore a fresh InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            // Create a new context.
            var dbContext = new ApplicationDbContext(options);
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            // Arrange
            var dbName = "GetAllAsync_ReturnsAllBooks"; // Ensure each test has a unique DB name
            await using var context = CreateDbContext(dbName);

            context.Books.AddRange(
                new Book { Title = "1984", Price = 19.84m, QuantityAvailable = 5 },
                new Book { Title = "Brave New World", Price = 15.99m, QuantityAvailable = 3 }
            );

            await context.SaveChangesAsync();

            // We'll create a repository with the in-memory context
            var repository = new Repository<Book>(context);

            // Act
            var books = await repository.GetAllAsync();

            // Assert
            var bookList = books.ToList();
            Assert.Equal(2, bookList.Count);
            Assert.Contains(bookList, b => b.Title == "1984");
            Assert.Contains(bookList, b => b.Title == "Brave New World");
        }

        [Fact]
        public async Task GetByIdAsync_WhenBookExists_ReturnsBook()
        {
            // Arrange
            var dbName = "GetByIdAsync_WhenBookExists_ReturnsBook";
            var context = CreateDbContext(dbName);
            var book = new Book { Id = 1, Title = "1984", Price = 19.84m, QuantityAvailable = 5 };
            context.Books.Add(book);
            await context.SaveChangesAsync();
            var repository = new Repository<Book>(context);

            // Act
            var foundBook = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(foundBook);
            Assert.Equal(book.Id, foundBook.Id);
            context.Dispose();
        }

        [Fact]
        public async Task CreateAsync_AddsBookToDatabase()
        {
            // Arrange
            var dbName = "CreateAsync_AddsBookToDatabase";
            var context = CreateDbContext(dbName);
            var repository = new Repository<Book>(context);
            var book = new Book { Title = "New Book", Price = 25.99m, QuantityAvailable = 10 };

            // Act
            await repository.CreateAsync(book);
            await repository.SaveAsync();

            // Assert
            var foundBook = await context.Books.Where(b => b.Title == "New Book").FirstOrDefaultAsync();
            Assert.NotNull(foundBook);
            Assert.Equal(book.Title, foundBook.Title);
            context.Dispose();
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingBook()
        {
            // Arrange
            var dbName = "UpdateAsync_UpdatesExistingBook";
            var context = CreateDbContext(dbName);
            var book = new Book { Id = 1, Title = "Original Book", Price = 19.84m, QuantityAvailable = 5 };
            context.Books.Add(book);
            await context.SaveChangesAsync();
            var repository = new Repository<Book>(context);

            // Retrieve the book to be updated from the context
            var bookToUpdate = await context.Books.FindAsync(1);
            bookToUpdate.Title = "Updated Book";
            bookToUpdate.Price = 19.99m;
            bookToUpdate.QuantityAvailable = 10;

            // Act
            repository.UpdateAsync(bookToUpdate);  // The test does not need to be "await" because UpdateAsync is not asynchronous
            await repository.SaveAsync();

            // Assert
            var updatedBook = await context.Books.FindAsync(1);
            Assert.Equal("Updated Book", updatedBook.Title);
            Assert.Equal(19.99m, updatedBook.Price);
            Assert.Equal(10, updatedBook.QuantityAvailable);

            context.Dispose();
        }

        [Fact]
        public async Task DeleteAsync_RemovesBookFromDatabase()
        {
            // Arrange
            var dbName = "DeleteAsync_RemovesBookFromDatabase";
            var context = CreateDbContext(dbName);
            var book = new Book { Id = 1, Title = "Book to Delete", Price = 19.84m, QuantityAvailable = 5 };
            context.Books.Add(book);
            await context.SaveChangesAsync();
            var repository = new Repository<Book>(context);

            // Act
            await repository.DeleteAsync(book.Id);
            await repository.SaveAsync();

            // Assert
            var foundBook = await context.Books.FindAsync(book.Id);
            Assert.Null(foundBook);
            context.Dispose();
        }

        [Fact]
        public async Task SearchBooksAsync_ReturnsFilteredBooks()
        {
            // Arrange
            var dbName = "SearchBooksAsync_ReturnsFilteredBooks";
            var context = CreateDbContext(dbName);

            var author1 = new Author { Name = "Aldous Huxley" };
            var genre1 = new Genre { Name = "Science Fiction" };

            var author2 = new Author { Name = "George Orwell" };
            var genre2 = new Genre { Name = "Dystopian" };

            context.Authors.AddRange(author1, author2);
            context.Genres.AddRange(genre1, genre2);
            await context.SaveChangesAsync();

            context.Books.AddRange(
                new Book { Title = "Brave New World", Author = author1, Genre = genre1 },
                new Book { Title = "1984", Author = author2, Genre = genre2 },
                new Book { Title = "The Great Gatsby", Author = new Author { Name = "F. Scott Fitzgerald" }, Genre = new Genre { Name = "Fiction" } }
            );
            await context.SaveChangesAsync();
            var repository = new BookRepository(context);

            // Act
            var result = await repository.SearchBooksAsync("dystopian");

            // Assert
            Assert.Single(result);
            Assert.Contains(result, b => b.Author.Name == "George Orwell");

            context.Dispose();
        }
    }
}