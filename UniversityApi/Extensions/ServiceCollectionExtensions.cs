using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Mapping;
using UniversityApi.Repositories.Implementations;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Implementations;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Extensions;

/// <summary>Регистрация зависимостей приложения в DI-контейнере.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// DbContext регистрируется как Scoped (по умолчанию у AddDbContext):
    /// один экземпляр на HTTP-запрос. DbContext не потокобезопасен и хранит Change Tracker,
    /// поэтому Singleton недопустим, а Transient дал бы разные контексты в разных репозиториях одного запроса.
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("Database:Provider") ?? DatabaseProviders.SqlServer;

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (provider.Equals(DatabaseProviders.PostgreSql, StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(configuration.GetConnectionString("PostgreSql"));
            }
            else
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
            }
        });

        return services;
    }

    /// <summary>
    /// Репозитории и сервисы — Scoped: они зависят от Scoped DbContext,
    /// поэтому должны жить столько же (один запрос). Singleton «захватил» бы DbContext навсегда.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repository layer
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

        // Service layer (бизнес-логика)
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<UserDtoFactory>();

        // AutoMapper: конфигурация маппинга строится один раз (Singleton внутри AddAutoMapper),
        // IMapper выдаётся из контейнера. Лицензионный ключ необязателен для учебного проекта.
        services.AddAutoMapper(cfg =>
        {
            var licenseKey = configuration["AutoMapper:LicenseKey"];
            if (!string.IsNullOrWhiteSpace(licenseKey))
                cfg.LicenseKey = licenseKey;

            cfg.AddProfile<MappingProfile>();
        });

        return services;
    }

    /// <summary>
    /// Ошибки валидации DTO ([Required], [EmailAddress], [Range] ...) превращаются
    /// в единый ReturnResult с кодом 400 и списком ошибок по полям.
    /// </summary>
    public static IMvcBuilder ConfigureValidationResponse(this IMvcBuilder builder)
    {
        return builder.ConfigureApiBehaviorOptions(options =>
        {
            // Не подменять наши ответы стандартным ProblemDetails
            options.SuppressMapClientErrors = true;

            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = new Dictionary<string, string[]>();

                foreach (var (key, entry) in context.ModelState)
                {
                    if (entry.Errors.Count == 0)
                        continue;

                    // Ключи вида "$.birthDate" появляются, когда JSON не удалось разобрать
                    // (неверный тип, формат даты, синтаксис). Текст таких ошибок содержит
                    // внутренние имена типов .NET, поэтому заменяем его на нейтральный.
                    var isJsonError = key.StartsWith('$');
                    var field = isJsonError ? key.TrimStart('$', '.') : key;
                    if (string.IsNullOrEmpty(field))
                        field = "body";
                    field = ToCamelCase(field); // имена полей как в JSON: firstName, birthDate

                    var messages = entry.Errors
                        .Select(e => isJsonError || string.IsNullOrEmpty(e.ErrorMessage)
                            ? "Invalid value or JSON format"
                            : e.ErrorMessage)
                        .Distinct()
                        .ToArray();

                    errors[field] = messages;
                }

                // Сообщение "The dto field is required." дублирует JSON-ошибку — убираем его
                if (errors.Keys.Any(k => k != "dto" && k != "filter"))
                {
                    errors.Remove("dto");
                }
                else if (errors.Remove("dto"))
                {
                    errors["body"] = new[] { "Request body is required" };
                }

                var http = context.HttpContext;
                var logger = http.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("UniversityApi.Validation");
                logger.LogWarning("Validation failed for {Method} {Path}: {Fields}",
                    http.Request.Method, http.Request.Path, string.Join(", ", errors.Keys));

                var result = ReturnResult<object>.BadRequest(ErrorCodes.ValidationError,
                    "One or more validation errors occurred", errors);
                result.TraceId = http.TraceIdentifier;

                return new BadRequestObjectResult(result);
            };
        });
    }

    private static string ToCamelCase(string name)
        => string.Join('.', name.Split('.').Select(part =>
            part.Length == 0 || char.IsLower(part[0]) ? part : char.ToLowerInvariant(part[0]) + part[1..]));
}

public static class DatabaseProviders
{
    public const string SqlServer = "SqlServer";
    public const string PostgreSql = "PostgreSql";
}
