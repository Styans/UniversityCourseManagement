using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

/// <summary>
/// Базовый контроллер: превращает ReturnResult&lt;T&gt; в HTTP-ответ с нужным статус-кодом
/// и подставляет TraceId текущего запроса.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(ReturnResult<T> result)
    {
        result.TraceId = HttpContext.TraceIdentifier;
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Для успешного создания возвращает 201 Created и заголовок Location с адресом нового ресурса.
    /// </summary>
    protected IActionResult ToCreatedResult<T>(ReturnResult<T> result, string actionName, Func<T, object> routeValues)
    {
        result.TraceId = HttpContext.TraceIdentifier;

        if (!result.IsSuccess || result.Result is null)
            return StatusCode(result.StatusCode, result);

        return CreatedAtAction(actionName, routeValues(result.Result), result);
    }
}
