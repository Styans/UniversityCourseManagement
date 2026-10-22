using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityApi.Models;

namespace UniversityApi.Data.Configurations;

/// <summary>
/// Связь учётной записи Identity с университетскими сущностями.
/// Один Student (Teacher) — не больше одной учётной записи (уникальный индекс по FK).
/// При удалении студента или преподавателя учётная запись остаётся, связь обнуляется.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(u => u.Student)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Teacher)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.TeacherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(u => u.CreatedAt).IsRequired();
    }
}
