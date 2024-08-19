using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBookstore.Models
{
    public class Author
    {
        [Key, Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [NotMapped]
        public string FullName { get => $"{Surname} {Name}"; }
        public Author()
        {

        }
        public Author(string code, string name, string surname):this()
        {
            Code = code;
            Name = name;
            Surname = surname;
        }
    }
}
