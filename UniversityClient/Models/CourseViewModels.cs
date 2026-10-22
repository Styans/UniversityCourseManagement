namespace UniversityClient.Models;

/// <summary>Курс для отображения. Поля v2 (кафедра, количество студентов) заполнены только для v2.</summary>
public class CourseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public int? EnrolledStudentsCount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CourseViewModel From(CourseV1Dto c) => new()
    {
        Id = c.Id, Name = c.Name, Description = c.Description, Credits = c.Credits,
        TeacherName = c.TeacherFullName, CreatedAt = c.CreatedAt
    };

    public static CourseViewModel From(CourseV2Dto c) => new()
    {
        Id = c.Id, Name = c.Name, Description = c.Description, Credits = c.Credits,
        TeacherName = c.Teacher.FullName, Department = c.Teacher.Department,
        EnrolledStudentsCount = c.EnrolledStudentsCount, CreatedAt = c.CreatedAt
    };
}

/// <summary>Описание ошибки API для частичного представления _ApiError.</summary>
public class ApiErrorViewModel
{
    public ApiErrorKind Kind { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? TraceId { get; set; }

    public string Title => Kind switch
    {
        ApiErrorKind.Unavailable => "API недоступен",
        ApiErrorKind.Timeout => "API не отвечает",
        ApiErrorKind.NotFound => "Не найдено",
        ApiErrorKind.InvalidResponse => "Некорректный ответ API",
        _ => $"Ошибка API{(StatusCode is null ? string.Empty : $" ({StatusCode})")}"
    };

    public static ApiErrorViewModel From<T>(ApiResult<T> result) => new()
    {
        Kind = result.ErrorKind, Message = result.Message, StatusCode = result.StatusCode,
        ErrorCode = result.ErrorCode, TraceId = result.TraceId
    };
}

public class CourseListViewModel
{
    public string Version { get; set; } = "v2";
    public string? Search { get; set; }
    public List<CourseViewModel> Courses { get; set; } = new();
    public ApiErrorViewModel? Error { get; set; }
}

public class CourseDetailsViewModel
{
    public string Version { get; set; } = "v2";
    public int Id { get; set; }
    public CourseViewModel? Course { get; set; }
    public ApiErrorViewModel? Error { get; set; }
}

/// <summary>Переключатель версии API (частичное представление _VersionSwitch).</summary>
public record VersionSwitchViewModel(string Version, string Action, int? Id, string? Search);
