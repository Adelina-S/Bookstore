using System.ComponentModel.DataAnnotations;

namespace WebBookstore.Models
{
    public class BookCard
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public Author Author { get; set; }
        [Required]
        public BookCategory Category { get; set; }
        public int Year { get; set; }
        public BookCard()
        {

        }
        public BookCard(string title, string description, Author author, BookCategory category, int year):this()
        {
            Title = title;
            Description = description;
            Author = author;
            Category = category;
            Year = year;
        }

    }
}
