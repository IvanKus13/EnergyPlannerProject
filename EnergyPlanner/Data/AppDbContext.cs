using EnergyPlanner.Services;
using EnergyPlannerProject.Models;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Scheme> Schemes => Set<Scheme>();
    public DbSet<Accumulator> Accumulators => Set<Accumulator>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(user =>
        {
            user.Property(u => u.Email)
                .HasMaxLength(AuthValidator.MaxEmailLength);

            user.HasIndex(u => u.Email)
                .IsUnique();
        });
    }
}