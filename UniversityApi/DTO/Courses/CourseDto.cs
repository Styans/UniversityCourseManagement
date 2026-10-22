namespace UniversityApi.DTO.Courses;

/// <summary>Курс в ответе API (с именем закреплённого преподавателя).</summary>
public class CourseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public int TeacherId { get; set; }
    public string TeacherFullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
