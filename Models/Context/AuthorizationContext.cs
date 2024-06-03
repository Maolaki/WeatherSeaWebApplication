using Microsoft.EntityFrameworkCore;

namespace WeatherSeaWebApplication.Models
{
    public class AuthorizationContext : DbContext
    {
        public AuthorizationContext(DbContextOptions<AuthorizationContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> UserList { get; set; }
    }
}
