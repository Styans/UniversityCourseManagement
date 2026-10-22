namespace UniversityApi.DTO.Enrollments;

/// <summary>Запись студента на курс в ответе API.</summary>
public class EnrollmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentFullName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public decimal? Grade { get; set; }
}
