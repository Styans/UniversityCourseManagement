using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UniversityApi.Auth;

/// <summary>
/// Параметры JWT из секции "Jwt".
/// Issuer, Audience и срок жизни лежат в appsettings.json,
/// а ключ подписи Key — только в User Secrets (Development) или переменной окружения Jwt__Key (Production).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>Минимальная длина ключа HMAC-SHA256 — 256 бит.</summary>
    public const int MinKeyBytes = 32;

    /// <summary>Издатель токена (кто выдал).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Аудитория токена (для кого выдан).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Секретный ключ подписи. Не хранится в appsettings.json и Git.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Срок действия Access Token в минутах.</summary>
    public int AccessTokenMinutes { get; set; } = 15;

    public SymmetricSecurityKey GetSigningKey() => new(Encoding.UTF8.GetBytes(Key));

    public bool IsKeyValid() => !string.IsNullOrWhiteSpace(Key) && Encoding.UTF8.GetByteCount(Key) >= MinKeyBytes;
}
