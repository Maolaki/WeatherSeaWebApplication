using Microsoft.EntityFrameworkCore;

namespace WeatherSeaWebApplication.Models
{
    public class ModulesContext : DbContext
    {
        public ModulesContext(DbContextOptions<ModulesContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessModel>()
                .HasKey(am => new { am.FieldId, am.UserLogin });
        }

        public DbSet<FieldModel> FieldList { get; set; }
        public DbSet<EntityModel> EntityList { get; set; }
        public DbSet<ReportModel> ReportList { get; set; }
        public DbSet<AccessModel> AccessList { get; set; }
    }
}
