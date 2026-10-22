using System.Text.Json.Serialization;

namespace UniversityApi.Common;

/// <summary>
/// Единый формат ответа API. Все endpoints возвращают этот объект —
/// и при успехе, и при ошибке, — поэтому клиенту достаточно одной модели для разбора ответа.
/// </summary>
public class ReturnResult<T>
{
    /// <summary>HTTP-код ответа (дублируется в теле для удобства клиента).</summary>
    public int StatusCode { get; set; }

    public bool IsSuccess { get; set; }

    /// <summary>Полезные данные; при ошибке — null.</summary>
    public T? Result { get; set; }

    /// <summary>Машиночитаемый код ошибки, например STUDENT_NOT_FOUND.</summary>
    public string? ErrorCode { get; set; }

    /// <summary>Понятное человеку описание ошибки.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Ошибки валидации по полям. Выводится только при VALIDATION_ERROR.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; set; }

    /// <summary>Идентификатор запроса — по нему запись можно найти в логах.</summary>
    public string? TraceId { get; set; }

    public static ReturnResult<T> Success(T result, int statusCode = StatusCodes.Status200OK) => new()
    {
        StatusCode = statusCode,
        IsSuccess = true,
        Result = result
    };

    public static ReturnResult<T> Created(T result) => Success(result, StatusCodes.Status201Created);

    public static ReturnResult<T> Fail(int statusCode, string errorCode, string errorMessage,
        IDictionary<string, string[]>? errors = null) => new()
    {
        StatusCode = statusCode,
        IsSuccess = false,
        Result = default,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        Errors = errors
    };

    public static ReturnResult<T> NotFound(string errorCode, string errorMessage)
        => Fail(StatusCodes.Status404NotFound, errorCode, errorMessage);

    public static ReturnResult<T> Conflict(string errorCode, string errorMessage)
        => Fail(StatusCodes.Status409Conflict, errorCode, errorMessage);

    public static ReturnResult<T> Unauthorized(string errorCode, string errorMessage)
        => Fail(StatusCodes.Status401Unauthorized, errorCode, errorMessage);

    public static ReturnResult<T> Forbidden(string errorCode, string errorMessage)
        => Fail(StatusCodes.Status403Forbidden, errorCode, errorMessage);

    public static ReturnResult<T> BadRequest(string errorCode, string errorMessage,
        IDictionary<string, string[]>? errors = null)
        => Fail(StatusCodes.Status400BadRequest, errorCode, errorMessage, errors);
}
