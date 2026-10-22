namespace UniversityApi.DTO.Courses.V2;

/// <summary>
/// Курс в ответе API версии 2.
/// Отдельный DTO: контракт v1 (CourseDto) не меняется, поэтому клиенты первой версии продолжают работать.
/// Отличия от v1: преподаватель — вложенный объект с кафедрой, добавлено количество записанных студентов.
/// </summary>
public class CourseV2Dto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Сведения о преподавателе курса.</summary>
    public CourseTeacherV2Dto Teacher { get; set; } = new();

    /// <summary>Количество студентов, записанных на курс.</summary>
    public int EnrolledStudentsCount { get; set; }
}

/// <summary>Преподаватель в составе курса (v2). Email не передаётся.</summary>
public class CourseTeacherV2Dto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
