using System.ComponentModel.DataAnnotations;

namespace WebBookstore.Models
{
    public class BookMove
    {
        public int Id { get; set; }
        [Required]
        public Book Book { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime EndTime { get; set; }
        [Required]
        public User User { get; set; }
        public BookMoveType BlockType { get; set; }
        public List<AutoWarning> WarningsList { get; set; }
        public bool isFinished { get; set; }
        public BookMove()
        {
            WarningsList = new List<AutoWarning>();
        }
        public BookMove(Book book, User user, BookMoveType blockType, DateTime endTime):this()
        {
            Book = book;
            User = user;
            DateTime = DateTime.Now;
            BlockType = blockType;
            EndTime = endTime;
        }
    }
}
