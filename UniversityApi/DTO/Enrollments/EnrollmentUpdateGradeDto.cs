using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Enrollments;

/// <summary>Изменение оценки студента по курсу (100-балльная шкала).</summary>
public class EnrollmentUpdateGradeDto
{
    /// <example>87.5</example>
    [Required(ErrorMessage = "Grade is required")]
    [Range(typeof(decimal), "0", "100", ErrorMessage = "Grade must be between 0 and 100")]
    public decimal? Grade { get; set; }
}
