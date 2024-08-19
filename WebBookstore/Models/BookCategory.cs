using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBookstore.Models
{
    public class BookCategory
    {
        [Key, Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        public BookCategory Parent { get; set; }
        //[NotMapped]
        public List<BookCategory> Childs { get; set; }
        public override string ToString()
        {
            return $"{Name} {Code}";
        }
        public BookCategory()
        {
            Childs= new List<BookCategory>();
        }
        public BookCategory(string code, string name, BookCategory parent):this()
        {
            Code = code;
            Name = name;
            Parent = parent;
        }
    }
}
