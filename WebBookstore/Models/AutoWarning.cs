namespace WebBookstore.Models
{
    public class AutoWarning
    {
        public int Id { get; set;}
        public DateTime Date { get; set;}
        public BookMove BookMove { get; set;}
        public bool IsSent { get; set;}
        public AutoWarning()
        {
            
        }
        public AutoWarning(DateTime date, BookMove bookMove) : this()
        {
            Date = date;
            BookMove = bookMove;
        }
    }
}
