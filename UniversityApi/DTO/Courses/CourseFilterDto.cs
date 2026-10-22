using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Courses;

/// <summary>
/// Параметры фильтрации курсов (передаются в query string):
/// GET /api/courses?search=data&amp;teacherId=2&amp;minCredits=4&amp;maxCredits=6&amp;department=Mathematics
/// </summary>
public class CourseFilterDto : IValidatableObject
{
    /// <summary>Часть названия или описания курса (без учёта регистра).</summary>
    [StringLength(150)]
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>Только курсы указанного преподавателя.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a positive number")]
    [FromQuery(Name = "teacherId")]
    public int? TeacherId { get; set; }

    /// <summary>Кафедра преподавателя.</summary>
    [StringLength(150)]
    [FromQuery(Name = "department")]
    public string? Department { get; set; }

    /// <summary>Минимальное количество кредитов.</summary>
    [Range(1, 10, ErrorMessage = "MinCredits must be between 1 and 10")]
    [FromQuery(Name = "minCredits")]
    public int? MinCredits { get; set; }

    /// <summary>Максимальное количество кредитов.</summary>
    [Range(1, 10, ErrorMessage = "MaxCredits must be between 1 and 10")]
    [FromQuery(Name = "maxCredits")]
    public int? MaxCredits { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinCredits.HasValue && MaxCredits.HasValue && MinCredits > MaxCredits)
        {
            yield return new ValidationResult(
                "MinCredits cannot be greater than MaxCredits",
                new[] { nameof(MinCredits), nameof(MaxCredits) });
        }
    }
}
