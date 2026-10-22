using System.ComponentModel.DataAnnotations;
using UniversityApi.Common.Validation;

namespace UniversityApi.DTO.Students;

/// <summary>Данные для изменения студента (PUT — полная замена изменяемых полей).</summary>
public class StudentUpdateDto
{
    /// <example>Alex</example>
    [Required(ErrorMessage = "FirstName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "FirstName must be 2-100 characters long")]
    public string FirstName { get; set; } = string.Empty;

    /// <example>Johnson</example>
    [Required(ErrorMessage = "LastName is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "LastName must be 2-100 characters long")]
    public string LastName { get; set; } = string.Empty;

    /// <example>alex.johnson@student.university.kz</example>
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
