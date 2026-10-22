using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityApi.Models;

namespace UniversityApi.Data.Configurations;

/// <summary>
/// Fluent API-конфигурация сущности Enrollment (связь многие-ко-многим Student ↔ Course).
/// </summary>
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    // SQL Server и PostgreSQL по-разному экранируют имена столбцов: [Credits] и "Credits".
    // Функцию передаёт ApplicationDbContext в зависимости от провайдера.
    private readonly Func<string, string> _quote;

    public EnrollmentConfiguration(Func<string, string> quote)
    {
        _quote = quote;
    }

    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", t =>
            t.HasCheckConstraint("CK_Enrollments_Grade", $"{_quote("Grade")} IS NULL OR ({_quote("Grade")} >= 0 AND {_quote("Grade")} <= 100)"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrollmentDate)
            .IsRequired();

        builder.Property(e => e.Grade)
            .HasPrecision(5, 2);

        // Enrollment → Student. При удалении студента удаляются и его записи на курсы.
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollment → Course. При удалении курса удаляются записи на него.
        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Бизнес-правило №3 на уровне БД: студент не может быть записан на курс дважды.
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}
