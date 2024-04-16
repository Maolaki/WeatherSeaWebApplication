using Microsoft.EntityFrameworkCore;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Models
{
    public class ModulesContext : DbContext
    {
        public ModulesContext(DbContextOptions<ModulesContext> options)
            : base(options)
        {
        }

        public DbSet<FieldModel> FieldList { get; set; }
    }
}
