using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBookstore.Models
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        public BookCard Card { get; set; }
        [Required]
        public User Owner { get; set; }
        public decimal Price { get; set; }
        public bool IsSold {  get; set; }
        public bool IsBlocked { get; set; }
        [NotMapped]
        public string Status { get; set; }
        [NotMapped]
        public bool IsReserved { get; set; }
        [NotMapped]
        public bool IsMoved { get; set; }
        public Book()
        {

        }
        public Book(BookCard card, User owner, decimal price):this()
        {
            Card = card;
            Owner = owner;
            Price = price;
        }

    }
}
