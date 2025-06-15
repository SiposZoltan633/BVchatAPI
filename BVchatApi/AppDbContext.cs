using Microsoft.EntityFrameworkCore;
using BVchatApi.Models;

namespace BVchatApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Felhasznalo> felhasznalok { get; set; }
    }
}