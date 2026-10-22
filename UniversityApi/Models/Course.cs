namespace UniversityApi.Models;

/// <summary>
/// Учебный курс. Атрибутов здесь нет специально:
/// ключ, ограничения и связи задаются через Fluent API в Data/Configurations/CourseConfiguration.cs.
/// </summary>
public class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Credits { get; set; }

    /// <summary>Внешний ключ на преподавателя.</summary>
    public int TeacherId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
