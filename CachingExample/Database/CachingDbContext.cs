using CachingExample.Entities;
using Microsoft.EntityFrameworkCore;

namespace CachingExample.Database;

public class CachingDbContext(DbContextOptions<CachingDbContext> options)
    : DbContext(options)
{
    #region PROPS :
    public DbSet<Driver> Drivers { get; set; }
    #endregion

    #region Methods :
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("CACHING");

        modelBuilder.Entity<Driver>()
            .ToTable("Drivers");

        modelBuilder.Entity<Driver>()
            .HasKey(x => x.Id);
    }
    #endregion
}
