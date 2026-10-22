using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityApi.Models;

namespace UniversityApi.Data.Configurations;

/// <summary>
/// Fluent API-конфигурация сущности Course.
/// </summary>
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    // SQL Server и PostgreSQL по-разному экранируют имена столбцов: [Credits] и "Credits".
    // Функцию передаёт ApplicationDbContext в зависимости от провайдера.
    private readonly Func<string, string> _quote;

    public CourseConfiguration(Func<string, string> quote)
    {
        _quote = quote;
    }

    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", t =>
            t.HasCheckConstraint("CK_Courses_Credits", $"{_quote("Credits")} >= 1 AND {_quote("Credits")} <= 10"));

        // Primary Key
        builder.HasKey(c => c.Id);

        // Обязательные поля и длина строк
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Credits)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Связь Course (много) → Teacher (один), внешний ключ TeacherId.
        // Restrict: нельзя удалить преподавателя, пока за ним закреплены курсы
        // (API в этом случае вернёт 409 Conflict).
        builder.HasOne(c => c.Teacher)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индекс для фильтрации курсов по названию
        builder.HasIndex(c => c.Name);
    }
}
