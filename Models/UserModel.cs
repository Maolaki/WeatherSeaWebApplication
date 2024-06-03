using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherSeaWebApplication.Models
{
    public enum UserType
    {
        [PgName("standart")]
        Standart, // Стандартный пакет
        [PgName("premium")]
        Premium, // Премиумный пакет
    }

    [Table("userlist")]
    public class UserModel
    {
        [Column("login")]
        [Key]
        public string Login { get; set; } = "";
        [Column("hash_password")]
        public string Password { get; set; } = "";
        [Column("username")]
        public string Username { get; set; } = "";
        [Column("email")]
        public string Email { get; set; } = "";
        [Column("type")]
        public UserType Type { get; set; }
        [Column("days")]
        public int Days { get; set; }

        public UserModel() { }
    }
}
