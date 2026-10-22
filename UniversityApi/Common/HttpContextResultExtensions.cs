using System.Text.Json;

namespace UniversityApi.Common;

/// <summary>Запись ReturnResult в ответ вне контроллеров (middleware, события аутентификации).</summary>
public static class HttpContextResultExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteReturnResultAsync(this HttpContext http, ReturnResult<object> result)
    {
        result.TraceId = http.TraceIdentifier;

        http.Response.StatusCode = result.StatusCode;
        http.Response.ContentType = "application/json; charset=utf-8";
        return http.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
    }
}
