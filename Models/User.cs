using System.ComponentModel.DataAnnotations;

namespace CarSell.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
    }

}
