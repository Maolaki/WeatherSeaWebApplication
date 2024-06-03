using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherSeaWebApplication.Models
{
    public enum AccessType
    {
        [PgName("view")]
        View = 0, // Только просмотр
        [PgName("edit")]
        Edit = 1, // Изменение
    }

    [Table("accesslist")]
    public class AccessModel
    {
        [Column("id_field")]
        [ForeignKey("FieldModel")]
        public int FieldId { get; set; }
        [Column("login_user")]
        [ForeignKey("UserModel")]
        public string UserLogin { get; set; } = "";
        [Column("type", TypeName = "accesstype")]
        public AccessType Type { get; set; }

        public AccessModel() { }

        public AccessModel(int fieldId, string userLogin, AccessType type)
        {
            FieldId = fieldId;
            UserLogin = userLogin;
            Type = type;
        }
    }
}
