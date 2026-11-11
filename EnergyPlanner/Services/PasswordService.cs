using EnergyPlannerProject.Models;
using Microsoft.AspNetCore.Identity;

namespace EnergyPlanner.Services;

public enum PasswordCheckResult
{
    Failed,

    Success,

    SuccessRehashNeeded
}

/// <summary>
/// Хешування та перевірка паролів.
///
/// Використовує стандартний PasswordHasher з ASP.NET Core Identity
/// (PBKDF2 з випадковою сіллю). У базі зберігається лише хеш, тож навіть
/// при витоку бази паролі користувачів не розкриваються.
/// </summary>
public class PasswordService
{
    private const byte HashV3Marker = 0x01;
    private const int MinHashV3Bytes = 15;

    private const byte HashV2Marker = 0x00;
    private const int HashV2Bytes = 49;

    private readonly IPasswordHasher<User> _hasher;

    public PasswordService(IPasswordHasher<User> hasher)
    {
        _hasher = hasher;
    }

    public string HashPassword(User user, string password) =>
        _hasher.HashPassword(user, password);

    public PasswordCheckResult VerifyPassword(User user, string password)
    {
        if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(password))
            return PasswordCheckResult.Failed;

        if (LooksLikeHash(user.PasswordHash))
        {
            return _hasher.VerifyHashedPassword(user, user.PasswordHash, password) switch
            {
                PasswordVerificationResult.Success => PasswordCheckResult.Success,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordCheckResult.SuccessRehashNeeded,
                _ => PasswordCheckResult.Failed
            };
        }

        
        return user.PasswordHash == password
            ? PasswordCheckResult.SuccessRehashNeeded
            : PasswordCheckResult.Failed;
    }

    /// <summary>
    /// Чи є збережене значення хешем, створеним PasswordHasher, а не відкритим паролем.
    /// </summary>
    public static bool LooksLikeHash(string storedValue)
    {
        var buffer = new byte[storedValue.Length];

        if (!Convert.TryFromBase64String(storedValue, buffer, out var length))
            return false;

        if (length == 0)
            return false;

        return (buffer[0] == HashV3Marker && length >= MinHashV3Bytes)
            || (buffer[0] == HashV2Marker && length == HashV2Bytes);
    }
}