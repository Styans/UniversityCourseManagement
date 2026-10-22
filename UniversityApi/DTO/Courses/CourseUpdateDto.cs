using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Courses;

/// <summary>Данные для изменения курса (в том числе смены преподавателя).</summary>
public class CourseUpdateDto
{
    /// <example>Mobile Development</example>
    [Required(ErrorMessage = "Course name is required")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Course name must be 3-150 characters long")]
    public string Name { get; set; } = string.Empty;

    /// <example>Cross-platform apps with React Native and Expo</example>
    [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters long")]
    public string? Description { get; set; }

    /// <example>6</example>
    [Required(ErrorMessage = "Credits is required")]
    [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
    public int? Credits { get; set; }

    /// <example>1</example>
    [Required(ErrorMessage = "TeacherId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a positive number")]
    public int? TeacherId { get; set; }
}
