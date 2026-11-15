using System.Net.Http.Json;
using System.Text.Json;
using EnergyPlannerProject.DTO;

namespace EnergyPlanner.Tests;

public static class ApiClient
{
    public const string DefaultPassword = "Password123";

    public static async Task RegisterAndLogin(HttpClient client, string email)
    {
        var credentials = new { email, password = DefaultPassword };

        var register = await client.PostAsJsonAsync("/api/auth/register", credentials);
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", credentials);
        login.EnsureSuccessStatusCode();
    }

    public static CreateSchemeDto CreateValidScheme(string name)
    {
        return new CreateSchemeDto
        {
            Name = name,
            Accumulators = new List<CreateAccumulatorDto>
            {
                new CreateAccumulatorDto
                {
                    Name = "Акумулятор",
                    CapacityKilowattHours = 5,
                    EfficiencyPercent = 90
                }
            },
            Devices = new List<CreateDeviceDto>
            {
                new CreateDeviceDto
                {
                    Name = "Холодильник",
                    PowerWatts = 150,
                    Quantity = 1,
                    HoursPerDay = 24
                },
                new CreateDeviceDto
                {
                    Name = "Роутер",
                    PowerWatts = 10,
                    Quantity = 1,
                    HoursPerDay = 1000
                }
            }
        };
    }

    public static async Task<int> SaveScheme(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/schemes", CreateValidScheme(name));
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("schemeId").GetInt32();
    }
}
