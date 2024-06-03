using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherSeaWebApplication.Models
{
    public enum EntityClass
    {
        [PgName("plant")]
        Plant = 0, // Растение
        [PgName("animal")]
        Animal = 1, // Животное
    }
    public enum EntityOrigin
    {
        [PgName("standart")]
        Standart, // Стандартная сущность
        [PgName("custom")]
        Custom, // Кастомная сущность
    }

    [Table("entitylist")]
    public class EntityModel
    {
        [Column("id_entity")]
        [Key]
        public int Id { get; set; }
        [Column("login_owner")]
        [ForeignKey("UserModel")]
        public string OwnerLogin { get; set; } = "";
        [Column("entity_class")]
        public EntityClass Class { get; set; }
        [Column("entity_origin")]
        public EntityOrigin Origin { get; set; }
        [Column("entity_name")]
        public string Name { get; set; } = "";
        [Column("description")]
        public string Description { get; set; } = "";
        [Column("recommended_temperature")]
        public double RecommendedTemperature { get; set; }
        [Column("recommended_wind_speed")]
        public double RecommendedWindSpeed { get; set; }
        [Column("recommended_humidity")]
        public double RecommendedHumidity { get; set; }

        public EntityModel() { }
    }
}
