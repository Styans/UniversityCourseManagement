using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Users;

/// <summary>
/// Пользователь в ответе API. Содержит только безопасные поля:
/// PasswordHash, SecurityStamp, ConcurrencyStamp, номер телефона и т. п. сюда не попадают.
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>Связанный студент (если есть).</summary>
    public int? StudentId { get; set; }
    public string? StudentFullName { get; set; }

    /// <summary>Связанный преподаватель (если есть).</summary>
    public int? TeacherId { get; set; }
    public string? TeacherFullName { get; set; }

    /// <summary>Учётная запись временно заблокирована после неудачных попыток входа.</summary>
    public bool IsLockedOut { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>Назначение роли пользователю.</summary>
public class AssignRoleDto
{
    /// <example>Teacher</example>
    [Required(ErrorMessage = "Role is required")]
    [StringLength(50, MinimumLength = 2)]
    public string Role { get; set; } = string.Empty;
}

/// <summary>Создание новой роли.</summary>
public class CreateRoleDto
{
    /// <example>Assistant</example>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be 2-50 characters long")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Role name may contain only latin letters")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Связь учётной записи с сущностью Student и/или Teacher.
/// null — убрать связь.
/// </summary>
public class LinkProfileDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive number")]
    public int? StudentId { get; set; }

    /// <example>null</example>
    [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a positive number")]
    public int? TeacherId { get; set; }
}
