using System.Globalization;
using System.Security.Claims;

namespace UniversityApi.Auth;

/// <summary>
/// Текущий пользователь запроса. Данные берутся из claims проверенного JWT
/// (HttpContext.User заполняет Authentication Middleware).
/// </summary>
public interface ICurrentUser
{
    ClaimsPrincipal Principal { get; }
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? UserName { get; }
    int? StudentId { get; }
    int? TeacherId { get; }
    bool IsInRole(string role);
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public ClaimsPrincipal Principal => _accessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public string? UserId => Principal.FindFirstValue(AppClaimTypes.UserId);

    public string? UserName => Principal.Identity?.Name;

    public int? StudentId => Principal.GetIntClaim(AppClaimTypes.StudentId);

    public int? TeacherId => Principal.GetIntClaim(AppClaimTypes.TeacherId);

    public bool IsInRole(string role) => Principal.IsInRole(role);
}

public static class ClaimsPrincipalExtensions
{
    public static int? GetIntClaim(this ClaimsPrincipal principal, string claimType)
        => int.TryParse(principal.FindFirstValue(claimType), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
}
