using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnergyPlannerProject.DTO;

namespace EnergyPlanner.Tests;

public class CapacityRecommendationTests
{
    private const string Url = "/api/calculations/capacity-recommendation";

    private static CapacityRecommendationRequestDto CreateValidRequest()
    {
        return new CapacityRecommendationRequestDto
        {
            TargetDays = 2,
            EfficiencyPercent = 90,
            Devices = new List<CreateDeviceDto>
            {
                new CreateDeviceDto
                {
                    Name = "Холодильник",
                    PowerWatts = 150,
                    Quantity = 1,
                    HoursPerDay = 24
                }
            },
            Accumulators = new List<CreateAccumulatorDto>
            {
                new CreateAccumulatorDto
                {
                    Name = "Акумулятор",
                    CapacityKilowattHours = 5,
                    EfficiencyPercent = 90
                }
            }
        };
    }

    private static async Task<HttpClient> CreateLoggedInClient(TestAppFactory factory)
    {
        var client = factory.CreateClient();
        await ApiClient.RegisterAndLogin(client, "planner@test.com");
        return client;
    }

    [Fact]
    public async Task ValidRequest_ReturnsRecommendation()
    {
        using var factory = new TestAppFactory();
        var client = await CreateLoggedInClient(factory);

        var response = await client.PostAsJsonAsync(Url, CreateValidRequest());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal(3600, root.GetProperty("dailyConsumption").GetDouble(), 3);
        Assert.Equal(7200, root.GetProperty("requiredEnergy").GetDouble(), 3);
        Assert.Equal(8, root.GetProperty("requiredCapacityKilowattHours").GetDouble(), 3);
        Assert.Equal(4500, root.GetProperty("currentAvailableEnergy").GetDouble(), 3);
        Assert.Equal(3, root.GetProperty("additionalCapacityKilowattHours").GetDouble(), 3);
        Assert.False(root.GetProperty("isCurrentCapacityEnough").GetBoolean());
    }

    [Fact]
    public async Task EnoughCapacity_ReturnsZeroAdditional()
    {
        using var factory = new TestAppFactory();
        var client = await CreateLoggedInClient(factory);

        var request = CreateValidRequest();
        request.Accumulators[0].CapacityKilowattHours = 10;

        var response = await client.PostAsJsonAsync(Url, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal(0, root.GetProperty("additionalCapacityKilowattHours").GetDouble(), 3);
        Assert.True(root.GetProperty("isCurrentCapacityEnough").GetBoolean());
    }

    [Fact]
    public async Task WithoutLogin_ReturnsUnauthorized()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(Url, CreateValidRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
