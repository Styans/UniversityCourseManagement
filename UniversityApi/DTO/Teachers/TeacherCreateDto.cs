using System.ComponentModel.DataAnnotations;
using UniversityApi.Common.Validation;

namespace UniversityApi.DTO.Teachers;

/// <summary>Данные для создания преподавателя.</summary>
public class TeacherCreateDto
{
    /// <example>Elena</example>
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be 2-100 characters long")]
    public string FirstName { get; set; } = string.Empty;

    /// <example>Petrova</example>
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "LastName must be 2-100 characters long")]
    public string LastName { get; set; } = string.Empty;

    /// <example>e.petrova@university.kz</example>
    [Required(ErrorMessage = "Email is required")]
    [StringLength(255)]
    [EmailAddress(ErrorMessage = ValidationPatterns.EmailMessage)]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = ValidationPatterns.EmailMessage)]
    public string Email { get; set; } = string.Empty;

    /// <example>Software Engineering</example>
    [Required(ErrorMessage = "Department is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Department must be 2-150 characters long")]
    public string Department { get; set; } = string.Empty;
}
