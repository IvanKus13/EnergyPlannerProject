using EnergyPlannerProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EnergyPlanner.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Accumulator> Accumulators => Set<Accumulator>();
}