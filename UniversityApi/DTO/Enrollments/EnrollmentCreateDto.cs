using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Enrollments;

/// <summary>Запись студента на курс. Дата записи ставится сервером, оценка — позже отдельным запросом.</summary>
public class EnrollmentCreateDto
{
    /// <example>1</example>
    [Required(ErrorMessage = "StudentId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive number")]
    public int? StudentId { get; set; }

    /// <example>3</example>
    [Required(ErrorMessage = "CourseId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive number")]
    public int? CourseId { get; set; }
}
