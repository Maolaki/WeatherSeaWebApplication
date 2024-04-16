namespace WeatherSeaWebApplication.Models
{
    public enum FieldType
    {
        Plant, // Только растения
        Animal, // Только животные
        Combined // Комбинированные
    }

    public class FieldModel
    {
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public FieldType Type { get; set; }
        public string Description { get; set; }
        public double CoordX { get; set; }
        public double CoordY { get; set; }

        // Конструктор с параметрами
        public FieldModel(int id, string name, FieldType type, string description, double coordX, double coordY)
        {
            OwnerId = id;
            Name = name;
            Type = type;
            Description = description;
            CoordX = coordX;
            CoordY = coordY;
        }
    }
}
