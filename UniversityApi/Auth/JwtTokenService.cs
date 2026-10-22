using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UniversityApi.Models;

namespace UniversityApi.Auth;

public interface ITokenService
{
    /// <summary>Выдаёт Access Token для пользователя с указанными ролями.</summary>
    AccessToken CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    /// <summary>Только для демонстрации (Development): корректно подписанный, но уже просроченный токен.</summary>
    AccessToken CreateExpiredAccessToken(ApplicationUser user, IEnumerable<string> roles);
}

public sealed record AccessToken(string Token, DateTime ExpiresAt);

/// <summary>
/// Формирование JWT. В токен попадают: id пользователя (sub), имя (name), роли (role),
/// связь с Student/Teacher (student_id / teacher_id) и уникальный id токена (jti).
/// Пароль, хеш пароля и другие секреты в токен никогда не записываются —
/// payload JWT лишь закодирован Base64Url, а не зашифрован.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccessToken CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var now = DateTime.UtcNow;
        return Create(user, roles, issuedAt: now, expires: now.AddMinutes(_settings.AccessTokenMinutes));
    }

    public AccessToken CreateExpiredAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var now = DateTime.UtcNow;
        return Create(user, roles, issuedAt: now.AddMinutes(-10), expires: now.AddMinutes(-5));
    }

    private AccessToken Create(ApplicationUser user, IEnumerable<string> roles, DateTime issuedAt, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new(AppClaimTypes.UserId, user.Id),
            new(AppClaimTypes.UserName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(AppClaimTypes.Role, role)));

        if (user.StudentId.HasValue)
            claims.Add(new Claim(AppClaimTypes.StudentId, user.StudentId.Value.ToString(CultureInfo.InvariantCulture)));

        if (user.TeacherId.HasValue)
            claims.Add(new Claim(AppClaimTypes.TeacherId, user.TeacherId.Value.ToString(CultureInfo.InvariantCulture)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_settings.GetSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(_handler.CreateToken(descriptor), expires);
    }
}
