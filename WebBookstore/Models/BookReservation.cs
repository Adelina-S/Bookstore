using System.ComponentModel.DataAnnotations;

namespace WebBookstore.Models
{
    public class BookReservation//Создаётся при бронировании книги пользователем. 
    {
        public int Id { get; set; }
        [Required]
        public Book Book { get; set; }
        [Required]
        public User User { get; set; }
        public BookMoveType MoveType { get; set; }
        public DateTime DateTime { get; set; }
        public bool isFinished { get; set; }
        public BookReservation()
        {

        }
        public BookReservation(Book book, User user, BookMoveType moveType):this()
        {
            Book = book;
            User = user;
            MoveType = moveType;
            DateTime = DateTime.Now;
        }
    }
}
