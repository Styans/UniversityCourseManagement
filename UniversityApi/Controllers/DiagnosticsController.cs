using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

/// <summary>
/// Служебный endpoint для демонстрации централизованной обработки ошибок (500).
/// Работает только в окружении Development; в остальных возвращает 404.
/// </summary>
[Route("api/diagnostics")]
public class DiagnosticsController : ApiControllerBase
{
    private readonly IWebHostEnvironment _env;

    public DiagnosticsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>Бросает необработанное исключение — middleware вернёт 500 без технических деталей.</summary>
    [HttpGet("error")]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowError()
    {
        if (!_env.IsDevelopment())
            return ToActionResult(ReturnResult<object>.NotFound(ErrorCodes.EndpointNotFound, "Endpoint not found"));

        throw new InvalidOperationException(
            "Demo exception: connection string 'Server=secret;Password=123' and SQL 'SELECT * FROM Students' must not reach the client");
    }
}
