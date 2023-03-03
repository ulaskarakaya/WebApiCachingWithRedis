using Microsoft.EntityFrameworkCore;
using WebApiCachingWithRedis.Models;

namespace WebApiCachingWithRedis.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
    }
}
