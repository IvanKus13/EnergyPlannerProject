using EnergyPlannerProject.Models;
using EnergyPlanner.Services;
using Microsoft.AspNetCore.Identity;

namespace EnergyPlanner.Tests;

public class PasswordServiceTests
{
    private readonly PasswordService _service = new(new PasswordHasher<User>());

    private static User CreateUser() => new() { Id = 1, Email = "user@mail.com" };

    [Fact]
    public void HashPassword_DoesNotStorePlainText()
    {
        var user = CreateUser();

        var hash = _service.HashPassword(user, "password1");

        Assert.NotEqual("password1", hash);
        Assert.True(PasswordService.LooksLikeHash(hash));
    }

    [Fact]
    public void HashPassword_GivesDifferentHashesForSamePassword()
    {
        var user = CreateUser();

        var first = _service.HashPassword(user, "password1");
        var second = _service.HashPassword(user, "password1");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void VerifyPassword_SucceedsForCorrectPassword()
    {
        var user = CreateUser();
        user.PasswordHash = _service.HashPassword(user, "password1");

        Assert.Equal(PasswordCheckResult.Success, _service.VerifyPassword(user, "password1"));
    }

    [Fact]
    public void VerifyPassword_FailsForWrongPassword()
    {
        var user = CreateUser();
        user.PasswordHash = _service.HashPassword(user, "password1");

        Assert.Equal(PasswordCheckResult.Failed, _service.VerifyPassword(user, "password2"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void VerifyPassword_FailsForEmptyPassword(string? password)
    {
        var user = CreateUser();
        user.PasswordHash = _service.HashPassword(user, "password1");

        Assert.Equal(PasswordCheckResult.Failed, _service.VerifyPassword(user, password!));
    }

    [Fact]
    public void VerifyPassword_FailsWhenUserHasNoPassword()
    {
        var user = CreateUser();
        user.PasswordHash = string.Empty;

        Assert.Equal(PasswordCheckResult.Failed, _service.VerifyPassword(user, "password1"));
    }

    [Theory]
    [InlineData("password1")]
    [InlineData("12345")]
    [InlineData("AAAA")]           
    public void VerifyPassword_AcceptsLegacyPlainTextAndRequestsRehash(string legacyPassword)
    {
        var user = CreateUser();
        user.PasswordHash = legacyPassword;

        Assert.Equal(
            PasswordCheckResult.SuccessRehashNeeded,
            _service.VerifyPassword(user, legacyPassword));
    }

    [Fact]
    public void VerifyPassword_FailsForLegacyUserWithWrongPassword()
    {
        var user = CreateUser();
        user.PasswordHash = "password1";

        Assert.Equal(PasswordCheckResult.Failed, _service.VerifyPassword(user, "password2"));
    }

    [Fact]
    public void VerifyPassword_RejectsHashUsedAsPassword()
    {
        var user = CreateUser();
        user.PasswordHash = _service.HashPassword(user, "password1");

        Assert.Equal(
            PasswordCheckResult.Failed,
            _service.VerifyPassword(user, user.PasswordHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not base64!")]
    [InlineData("AAAA")]
    public void LooksLikeHash_ReturnsFalseForNonHashes(string value)
    {
        Assert.False(PasswordService.LooksLikeHash(value));
    }
}