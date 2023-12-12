﻿namespace WebApplication2.Models
{
public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Book> Books { get; set; }
    }
}
