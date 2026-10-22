using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Служебный endpoint для демонстрации централизованной обработки ошибок (500).
/// Работает только в окружении Development; в остальных возвращает 404.
/// </summary>
[Route("api/diagnostics")]
public class DiagnosticsController : ApiControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IAuthService _auth;

    public DiagnosticsController(IWebHostEnvironment env, IAuthService auth)
    {
        _env = env;
        _auth = auth;
    }

    /// <summary>Бросает необработанное исключение — middleware вернёт 500 без технических деталей.</summary>
    [HttpGet("error")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowError()
    {
        if (!_env.IsDevelopment())
            return ToActionResult(ReturnResult<object>.NotFound(ErrorCodes.EndpointNotFound, "Endpoint not found"));

        throw new InvalidOperationException(
            "Demo exception: internal server name 'db-internal-01' and SQL 'SELECT * FROM Students' must not reach the client");
    }

    /// <summary>
    /// Выдаёт копию токена текущего пользователя, срок действия которой уже истёк (только Development).
    /// Нужен, чтобы показать сценарий «запрос с просроченным JWT → 401 TOKEN_EXPIRED», не дожидаясь 15 минут.
    /// </summary>
    [HttpPost("expired-token")]
    [Authorize]
    [ProducesResponseType(typeof(ReturnResult<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExpiredToken()
    {
        if (!_env.IsDevelopment())
            return ToActionResult(ReturnResult<object>.NotFound(ErrorCodes.EndpointNotFound, "Endpoint not found"));

        return ToActionResult(await _auth.CreateExpiredTokenForCurrentUserAsync());
    }
}
