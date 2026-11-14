using EnergyPlanner.Controllers;
using EnergyPlanner.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyPlanner.Tests;

public class TestAppFactory : WebApplicationFactory<SchemesController>
{
    private readonly string _databaseName = "EnergyPlannerTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var registrations = new List<ServiceDescriptor>();

            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>))
                    registrations.Add(descriptor);
            }

            foreach (var descriptor in registrations)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
