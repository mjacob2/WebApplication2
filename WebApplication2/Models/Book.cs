namespace WebApplication2.Models
{
public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public virtual Author Author { get; set; }
        public int AuthorId { get; set; }
        public virtual Genre Genre { get; set; }
        public int GenreId { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
