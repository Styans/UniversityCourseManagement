using System.ComponentModel.DataAnnotations;
using UniversityApi.Common.Validation;
using UniversityApi.DTO.Users;

namespace UniversityApi.DTO.Auth;

/// <summary>
/// Регистрация. Поля роли здесь нет специально: новый пользователь всегда получает роль Student,
/// повысить привилегии может только Admin через /api/users/{id}/roles.
/// </summary>
public class RegisterDto
{
    /// <example>new.student</example>
    [Required(ErrorMessage = "UserName is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "UserName must be 3-50 characters long")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "UserName may contain only latin letters, digits and . _ -")]
    public string UserName { get; set; } = string.Empty;

    /// <example>new.student@student.university.kz</example>
    [Required(ErrorMessage = "Email is required")]
    [StringLength(255)]
    [EmailAddress(ErrorMessage = ValidationPatterns.EmailMessage)]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = ValidationPatterns.EmailMessage)]
    public string Email { get; set; } = string.Empty;

    /// <example>Student#2026</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at most 100 characters long")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Вход по имени пользователя и паролю.</summary>
public class LoginDto
{
    /// <example>admin</example>
    [Required(ErrorMessage = "UserName is required")]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    /// <example>Demo#2026</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Ответ на успешный вход.</summary>
public class AuthResponseDto
{
    /// <summary>JWT Access Token. Передаётся в заголовке: Authorization: Bearer &lt;token&gt;.</summary>
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    /// <summary>Момент истечения токена (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    public UserDto User { get; set; } = new();
}
