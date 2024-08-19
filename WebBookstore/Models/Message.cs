namespace WebBookstore.Models
{
    public class Message
    {
        public int Id { get; set; }
        public User Target { get; set; }
        public string Text {  get; set; }
        public DateTime DateTime { get; set; }
        public bool IsDeleted { get; set; }
        public Book Book { get; set; }
        public Message()
        {

        }
        public Message(User target, string text, Book book):this()
        {
            Target = target; 
            Text = text;
            Book = book;
            DateTime = DateTime.Now;
        }
    }
}
