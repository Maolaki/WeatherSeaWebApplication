namespace WeatherSeaWebApplication.Models
{
    public class FieldAccessSummary
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = "";
        public int EditCount { get; set; }
        public int ViewCount { get; set; }
    }

}
