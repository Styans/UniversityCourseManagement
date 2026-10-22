namespace UniversityApi.Models;

/// <summary>
/// Запись студента на курс — промежуточная сущность связи многие-ко-многим Student ↔ Course.
/// Настраивается через Fluent API (Data/Configurations/EnrollmentConfiguration.cs).
/// </summary>
public class Enrollment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; }

    /// <summary>Оценка по 100-балльной шкале; null — оценка ещё не выставлена.</summary>
    public decimal? Grade { get; set; }

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
