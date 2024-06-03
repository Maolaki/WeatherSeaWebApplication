using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherSeaWebApplication.Models
{
    public enum FieldType
    {
        [PgName("plant")]
        Plant = 0, // Только растения
        [PgName("animal")]
        Animal = 1, // Только животные
        [PgName("combined")]
        Combined = 2 // Комбинированные
    }

    [Table("fieldlist")]
    public class FieldModel
    {
        [Column("id_field")]
        [Key]
        public int FieldId { get; set; }
        [Column("login_owner")]
        [ForeignKey("UserModel")]
        public string OwnerLogin { get; set; } = "";
        [Column("field_name")]
        public string Name { get; set; } = "";
        [Column("field_type", TypeName = "fieldtype")]
        public FieldType Type { get; set; }
        [Column("description")]
        public string Description { get; set; } = "";
        [Column("latitude")]
        public double Latitude { get; set; }
        [Column("longitude")]
        public double Longitude { get; set; }

        public FieldModel() { }
    }
}
