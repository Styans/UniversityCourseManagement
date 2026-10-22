using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;

namespace UniversityApi.Middleware;

/// <summary>
/// Централизованная обработка непредвиденных ошибок.
/// Клиент получает только ReturnResult с кодом ошибки и TraceId,
/// а полное исключение (StackTrace, SQL и т. п.) записывается в лог с уровнем Error.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        ReturnResult<object> result;

        if (exception is UnauthorizedAccessException)
        {
            // Нарушение прав, обнаруженное глубже контроллера, — не 500, а 403 в общем формате
            _logger.LogWarning("Access denied. TraceId={TraceId}, {Method} {Path}, User={UserName}",
                traceId, context.Request.Method, context.Request.Path, context.User.Identity?.Name);

            result = ReturnResult<object>.Forbidden(ErrorCodes.Forbidden,
                "You do not have permission to perform this action");
        }
        else if (exception is DbUpdateException)
        {
            // Например, два одновременных запроса прошли проверку и упёрлись в уникальный индекс БД
            _logger.LogError(exception,
                "Database update error. TraceId={TraceId}, {Method} {Path}",
                traceId, context.Request.Method, context.Request.Path);

            result = ReturnResult<object>.Conflict(ErrorCodes.DatabaseConflict,
                "The operation conflicts with the current state of the data");
        }
        else
        {
            _logger.LogError(exception,
                "Unhandled exception. TraceId={TraceId}, {Method} {Path}",
                traceId, context.Request.Method, context.Request.Path);

            result = ReturnResult<object>.Fail(StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalServerError, "An unexpected error occurred");
        }

        result.TraceId = traceId;

        context.Response.Clear();
        context.Response.StatusCode = result.StatusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
