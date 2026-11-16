using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnergyPlanner.Logging;
using EnergyPlanner.Middleware;
using Microsoft.Extensions.Logging;

namespace EnergyPlanner.Tests;

public class ErrorHandlingTests
{
    private static async Task<string> ReadMessage(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        return json.RootElement.GetProperty("message").GetString() ?? string.Empty;
    }

    [Fact]
    public async Task UnhandledException_Returns500_WithMessageWithoutDetails()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/test-errors/crash");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(GlobalExceptionHandler.ServerErrorMessage, await ReadMessage(response));
        Assert.DoesNotContain(ThrowingTestController.SecretDetail, body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Fact]
    public async Task ArgumentException_Returns400_WithMessage()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/test-errors/invalid-argument");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(GlobalExceptionHandler.InvalidInputMessage, await ReadMessage(response));
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsJsonMessage()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "not-an-email", password = ApiClient.DefaultPassword });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(await ReadMessage(response)));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsJsonMessage()
    {
        using var factory = new TestAppFactory();
        var client = factory.CreateClient();

        await ApiClient.RegisterAndLogin(client, "owner@test.com");

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "owner@test.com", password = "WrongPassword1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(await ReadMessage(response)));
    }

    [Fact]
    public void FileLogger_WritesErrorWithException()
    {
        var path = Path.Combine(Path.GetTempPath(), "energyplanner-tests", Guid.NewGuid() + ".log");

        using (var provider = new FileLoggerProvider(path))
        {
            var logger = provider.CreateLogger("Test");
            logger.LogError(new InvalidOperationException("boom"), "Помилка розрахунку");
        }

        var content = File.ReadAllText(path);

        Assert.Contains("[Error]", content);
        Assert.Contains("Помилка розрахунку", content);
        Assert.Contains("boom", content);
    }
}
