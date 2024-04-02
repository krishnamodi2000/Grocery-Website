using Besos.Models;
using Microsoft.EntityFrameworkCore;

namespace Besos.Data
{
    public class AppDbContext:DbContext 
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            
        }
    }
}
