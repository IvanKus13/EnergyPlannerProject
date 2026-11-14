using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EnergyPlanner.Tests;

public class DeleteSchemeTests
{
    [Fact]
    public async Task DeleteOwnScheme_RemovesSchemeAccumulatorsAndDevices()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        await ApiClient.RegisterAndLogin(client, "owner@test.com");
        var schemeId = await ApiClient.SaveScheme(client, "Квартира");

        var response = await client.DeleteAsync($"/api/schemes/{schemeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var context = factory.CreateDbContext();

        Assert.Equal(0, await context.Schemes.CountAsync());
        Assert.Equal(0, await context.Accumulators.CountAsync());
        Assert.Equal(0, await context.Devices.CountAsync());
    }

    [Fact]
    public async Task DeleteOwnScheme_DisappearsFromHistory()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        await ApiClient.RegisterAndLogin(client, "owner@test.com");
        var deletedId = await ApiClient.SaveScheme(client, "Квартира");
        await ApiClient.SaveScheme(client, "Дача");

        await client.DeleteAsync($"/api/schemes/{deletedId}");

        var history = await client.GetStringAsync("/api/schemes");
        using var json = JsonDocument.Parse(history);

        Assert.Equal(1, json.RootElement.GetArrayLength());
        Assert.Equal("Дача", json.RootElement[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task DeleteOtherUsersScheme_ReturnsNotFound_AndKeepsScheme()
    {
        using var factory = new TestAppFactory();

        var owner = factory.CreateClient();
        await ApiClient.RegisterAndLogin(owner, "owner@test.com");
        var schemeId = await ApiClient.SaveScheme(owner, "Квартира");

        var stranger = factory.CreateClient();
        await ApiClient.RegisterAndLogin(stranger, "stranger@test.com");

        var response = await stranger.DeleteAsync($"/api/schemes/{schemeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var context = factory.CreateDbContext();

        Assert.Equal(1, await context.Schemes.CountAsync());
        Assert.Equal(2, await context.Devices.CountAsync());
    }

    [Fact]
    public async Task DeleteMissingScheme_ReturnsNotFound()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        await ApiClient.RegisterAndLogin(client, "owner@test.com");

        var response = await client.DeleteAsync("/api/schemes/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWithoutLogin_ReturnsUnauthorized()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/schemes/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
