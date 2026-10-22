using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UniversityApi.Data.Configurations;
using UniversityApi.Models;

namespace UniversityApi.Data;

/// <summary>
/// Контекст базы данных. Каждый DbSet&lt;T&gt; соответствует таблице.
/// Конфигурация моделей:
///  • Conventions — Id как первичный ключ, имена FK, навигационные свойства;
///  • Data Annotations — Student, Teacher (атрибуты в классах моделей);
///  • Fluent API — Course, Enrollment (классы в Data/Configurations).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Func<string, string> quote = Database.IsNpgsql()
            ? column => $"\"{column}\""
            : column => $"[{column}]";

        // Fluent API-конфигурации из папки Data/Configurations
        modelBuilder.ApplyConfiguration(new CourseConfiguration(quote));
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration(quote));
    }

    /// <summary>
    /// Все DateTime хранятся в UTC. При чтении помечаем их как Utc,
    /// чтобы в JSON дата уходила с признаком часового пояса (…Z).
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetCreatedAt();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetCreatedAt();
        return base.SaveChanges();
    }

    /// <summary>
    /// Автоматически проставляет дату создания для новых объектов,
    /// чтобы клиент API не мог передать её сам.
    /// </summary>
    private void SetCreatedAt()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added)
                continue;

            switch (entry.Entity)
            {
                case Student student when student.CreatedAt == default:
                    student.CreatedAt = now;
                    break;
                case Course course when course.CreatedAt == default:
                    course.CreatedAt = now;
                    break;
                case Enrollment enrollment when enrollment.EnrollmentDate == default:
                    enrollment.EnrollmentDate = now;
                    break;
            }
        }
    }
}

/// <summary>Конвертер DateTime: в БД пишется UTC, из БД читается с Kind = Utc.</summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
