using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UniversityApi.Models;

/// <summary>
/// Студент. Модель настроена через Data Annotations (атрибуты прямо на свойствах)
/// и Conventions (Id → первичный ключ, коллекция Enrollments → связь один-ко-многим).
/// </summary>
[Table("Students")]
[Index(nameof(Email), IsUnique = true)]
public class Student
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Записи студента на курсы (связь многие-ко-многим через Enrollment).</summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
