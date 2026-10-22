using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UniversityApi.Models;

/// <summary>
/// Преподаватель. Настроен через Data Annotations.
/// </summary>
[Table("Teachers")]
[Index(nameof(Email), IsUnique = true)]
public class Teacher
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

    [Required]
    [MaxLength(150)]
    public string Department { get; set; } = string.Empty;

    /// <summary>Курсы, закреплённые за преподавателем (один-ко-многим).</summary>
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
