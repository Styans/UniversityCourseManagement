using Microsoft.AspNetCore.Identity;

namespace UniversityApi.Models;

/// <summary>
/// Учётная запись пользователя (ASP.NET Core Identity, таблица AspNetUsers).
/// Пароль хранится только в виде хеша (PasswordHash, PBKDF2) — его формирует Identity.
/// Учётная запись может быть связана с существующей сущностью Student или Teacher:
/// по этой связи проверяется, «свои» ли оценки и курсы запрашивает пользователь.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Связанный студент (для роли Student).</summary>
    public int? StudentId { get; set; }

    public Student? Student { get; set; }

    /// <summary>Связанный преподаватель (для роли Teacher).</summary>
    public int? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }

    public DateTime CreatedAt { get; set; }
}
