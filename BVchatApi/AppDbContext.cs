// AppDbContext.cs
using BVchatApi.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Például egy Felhasznalok tábla
    public DbSet<Felhasznalo> Felhasznalok { get; set; }
}