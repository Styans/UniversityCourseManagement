using System.ComponentModel.DataAnnotations;
using UniversityApi.Common.Validation;

namespace UniversityApi.DTO.Students;

/// <summary>Данные для создания студента. Id и CreatedAt клиент не передаёт — их задаёт сервер.</summary>
public class StudentCreateDto
{
    /// <example>Alex</example>
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be 2-100 characters long")]
    public string FirstName { get; set; } = string.Empty;

    /// <example>Smith</example>
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "LastName must be 2-100 characters long")]
    public string LastName { get; set; } = string.Empty;

    /// <example>alex.smith@student.university.kz</example>
    [Required(ErrorMessage = "Email is required")]
    [StringLength(255)]
    [EmailAddress(ErrorMessage = ValidationPatterns.EmailMessage)]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = ValidationPatterns.EmailMessage)]
    public string Email { get; set; } = string.Empty;

    /// <example>2004-05-15</example>
    [Required(ErrorMessage = "BirthDate is required")]
    [BirthDate(MinAge = 14, MaxAge = 100)]
    public DateOnly? BirthDate { get; set; }
}
