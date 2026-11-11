using System.Net.Mail;

namespace EnergyPlanner.Services;

/// <summary>
/// Перевірка та нормалізація даних для реєстрації і входу.
/// Не залежить від бази даних і HTTP, тому легко покривається юніт-тестами.
/// </summary>
public static class AuthValidator
{
    public const int MaxEmailLength = 254;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    /// <summary>
    /// Приводить пошту до єдиного вигляду, щоб "User@Mail.com " і "user@mail.com"
    /// вважалися однією адресою.
    /// </summary>
    public static string NormalizeEmail(string? email) =>
        (email ?? string.Empty).Trim().ToLowerInvariant();

    /// <summary>
    /// Повертає текст помилки або null, якщо адреса коректна.
    /// Очікує вже нормалізовану адресу.
    /// </summary>
    public static string? ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Введіть електронну пошту.";

        if (email.Length > MaxEmailLength)
            return $"Адреса пошти має бути не довшою за {MaxEmailLength} символи.";

        if (!MailAddress.TryCreate(email, out var address) || address.Address != email)
            return "Некоректна адреса електронної пошти.";

        var domain = address.Host;

        if (!domain.Contains('.') || domain.StartsWith('.') || domain.EndsWith('.'))
            return "Некоректна адреса електронної пошти.";

        return null;
    }

    /// <summary>
    /// Повертає текст помилки або null, якщо пароль відповідає вимогам.
    /// </summary>
    public static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return "Введіть пароль.";

        if (password.Length < MinPasswordLength)
            return $"Пароль має містити щонайменше {MinPasswordLength} символів.";

        if (password.Length > MaxPasswordLength)
            return $"Пароль має бути не довшим за {MaxPasswordLength} символів.";

        if (password.Any(char.IsWhiteSpace))
            return "Пароль не може містити пробілів.";

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            return "Пароль має містити хоча б одну літеру та одну цифру.";

        return null;
    }
}