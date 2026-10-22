namespace UniversityClient.Models;

/// <summary>Формат ответа API (ReturnResult&lt;T&gt;) — клиенту достаточно одной модели для разбора.</summary>
public class ApiEnvelope<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T? Result { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>Курс в контракте API v1.</summary>
public class CourseV1Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int TeacherId { get; set; }
    public string TeacherFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Курс в контракте API v2.</summary>
public class CourseV2Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }
    public CourseTeacherV2Dto Teacher { get; set; } = new();
    public int EnrolledStudentsCount { get; set; }
}

public class CourseTeacherV2Dto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

/// <summary>Почему запрос к API не удался — от этого зависит сообщение пользователю.</summary>
public enum ApiErrorKind
{
    None,
    /// <summary>Не удалось установить соединение (API не запущен, неверный адрес, сеть).</summary>
    Unavailable,
    /// <summary>API не ответил за отведённое время.</summary>
    Timeout,
    /// <summary>404 — курс не найден.</summary>
    NotFound,
    /// <summary>Любой другой неуспешный HTTP-код (400, 401, 403, 500 …).</summary>
    HttpError,
    /// <summary>Ответ не удалось разобрать.</summary>
    InvalidResponse
}

/// <summary>Результат вызова API для контроллеров клиента: данные или описание ошибки.</summary>
public class ApiResult<T>
{
    public bool IsSuccess => ErrorKind == ApiErrorKind.None;
    public T? Data { get; init; }
    public ApiErrorKind ErrorKind { get; init; }
    public int? StatusCode { get; init; }
    public string? ErrorCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? TraceId { get; init; }

    public static ApiResult<T> Ok(T data) => new() { Data = data };

    public static ApiResult<T> Fail(ApiErrorKind kind, string message, int? statusCode = null,
        string? errorCode = null, string? traceId = null)
        => new() { ErrorKind = kind, Message = message, StatusCode = statusCode, ErrorCode = errorCode, TraceId = traceId };
}
