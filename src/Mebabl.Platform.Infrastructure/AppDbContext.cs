using Microsoft.EntityFrameworkCore;

namespace Mebabl.Platform.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // أضف الـ DbSets الخاصة بجداولك هنا لاحقاً، مثلاً:
        // public DbSet<User> Users { get; set; }
    }
}