using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBookstore.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public Role Role { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public User()
        {

        }
        public User(string login, string password, string name) : this()
        {
            Name = name;
            Login = login;
            Password = password;
        }
    }
}
