using EnergyPlanner.Services;

namespace EnergyPlanner.Tests;

public class AuthValidatorTests
{

    [Theory]
    [InlineData("User@Mail.com", "user@mail.com")]
    [InlineData("  user@mail.com  ", "user@mail.com")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void NormalizeEmail_TrimsAndLowercases(string? input, string expected)
    {
        Assert.Equal(expected, AuthValidator.NormalizeEmail(input));
    }


    [Theory]
    [InlineData("user@mail.com")]
    [InlineData("first.last@knu.ua")]
    [InlineData("user+tag@sub.domain.org")]
    public void ValidateEmail_AcceptsCorrectAddresses(string email)
    {
        Assert.Null(AuthValidator.ValidateEmail(email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("@mail.com")]
    [InlineData("user@localhost")]
    [InlineData("user@mail.")]
    [InlineData("user @mail.com")]
    [InlineData("Ivan <ivan@mail.com>")]
    public void ValidateEmail_RejectsIncorrectAddresses(string email)
    {
        Assert.NotNull(AuthValidator.ValidateEmail(email));
    }

    [Fact]
    public void ValidateEmail_RejectsTooLongAddress()
    {
        var email = new string('a', AuthValidator.MaxEmailLength) + "@mail.com";

        Assert.NotNull(AuthValidator.ValidateEmail(email));
    }


    [Theory]
    [InlineData("password1")]
    [InlineData("Пароль2026")]
    [InlineData("a1b2c3d4")]
    public void ValidatePassword_AcceptsStrongEnoughPasswords(string password)
    {
        Assert.Null(AuthValidator.ValidatePassword(password));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc123")]          
    [InlineData("onlyletters")]     
    [InlineData("1234567890")]     
    [InlineData("pass word1")]      
    public void ValidatePassword_RejectsWeakPasswords(string? password)
    {
        Assert.NotNull(AuthValidator.ValidatePassword(password));
    }

    [Fact]
    public void ValidatePassword_AcceptsBoundaryLengths()
    {
        var shortest = "a1" + new string('b', AuthValidator.MinPasswordLength - 2);
        var longest = "a1" + new string('b', AuthValidator.MaxPasswordLength - 2);

        Assert.Null(AuthValidator.ValidatePassword(shortest));
        Assert.Null(AuthValidator.ValidatePassword(longest));
    }

    [Fact]
    public void ValidatePassword_RejectsJustOutsideBoundaries()
    {
        var tooShort = "a1" + new string('b', AuthValidator.MinPasswordLength - 3);
        var tooLong = "a1" + new string('b', AuthValidator.MaxPasswordLength - 1);

        Assert.NotNull(AuthValidator.ValidatePassword(tooShort));
        Assert.NotNull(AuthValidator.ValidatePassword(tooLong));
    }
}