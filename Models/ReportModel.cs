using NpgsqlTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeatherSeaWebApplication.Models
{
    public enum ReportType
    {
        [PgName("basic")]
        Basic = 0, // Без подробностей
        [PgName("plant")]
        Plant = 1, // Только растения
        [PgName("animal")]
        Animal = 2, // Только животные
        [PgName("combined")]
        Combined = 3 // Комбинированный
    }

    [Table("reportlist")]
    public class ReportModel
    {
        [Column("id_report")]
        [Key]
        public int Id { get; set; }
        [Column("report_name")]
        public string Name { get; set; } = "";
        [Column("report_type")]
        public ReportType Type { get; set; }
        [Column("id_field")]
        [ForeignKey("FieldModel")]
        public int FieldId { get; set; }

        public ReportModel() { }
    }
}
