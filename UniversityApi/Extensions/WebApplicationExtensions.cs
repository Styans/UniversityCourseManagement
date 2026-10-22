using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Data.Seed;

namespace UniversityApi.Extensions;

public static class WebApplicationExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Создаёт/обновляет схему БД и при необходимости заполняет её тестовыми данными.
    /// SQL Server — через миграции EF Core (папка Migrations),
    /// PostgreSQL — через EnsureCreated (схема строится по той же модели).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("UniversityApi.Database");

        // Проверка, что все свойства DTO/Entity покрыты маппингом (ошибка видна сразу при запуске)
        scope.ServiceProvider.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

        if (db.Database.IsSqlServer())
        {
            logger.LogInformation("Applying EF Core migrations (SQL Server)...");
            await db.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("Ensuring database is created ({Provider})...", db.Database.ProviderName);
            if (!await db.Database.EnsureCreatedAsync())
                await EnsureIdentityTablesAsync(db, logger);
        }

        if (app.Configuration.GetValue("Database:SeedTestData", true))
            await DbSeeder.SeedAsync(db, logger);

        // Роли создаются всегда; демо-пользователи — только если задан Seed:DemoUsersPassword (Development)
        await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration, logger);
    }

    /// <summary>
    /// PostgreSQL без миграций: если база создана ещё в задании 1 (EnsureCreated её не трогает),
    /// в ней нет таблиц Identity. Создаём только их — из того же скрипта, который строит EF Core по модели.
    /// </summary>
    private static async Task EnsureIdentityTablesAsync(ApplicationDbContext db, ILogger logger)
    {
        var exists = await db.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS \"Value\" FROM information_schema.tables WHERE table_name = 'AspNetUsers'")
            .SingleAsync();
        if (exists > 0)
            return; // таблицы Identity уже есть

        logger.LogInformation("Identity tables not found in existing database — creating them");

        var statements = Regex.Split(db.Database.GenerateCreateScript(), @";\s*\r?\n")
            .Select(sql => sql.Trim())
            .Where(sql => sql.Contains("\"AspNet", StringComparison.Ordinal));

        foreach (var sql in statements)
            await db.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    /// Ответы без тела с кодами 401/403/404/405/415 (нет токена, нет прав, несуществующий URL,
    /// неверный метод, не тот Content-Type) тоже приводятся к единому формату ReturnResult.
    /// </summary>
    private static readonly string[] SupportedVersions = { "1", "2" };
    private static readonly Regex VersionSegment = new(@"^/api/v(?<v>\d+)(\.0)?/", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Версия из адреса /api/vN/..., если такой версии API нет (например, v3).</summary>
    private static string? RequestedUnsupportedVersion(PathString path)
    {
        var match = VersionSegment.Match(path.Value ?? string.Empty);
        return match.Success && !SupportedVersions.Contains(match.Groups["v"].Value) ? match.Groups["v"].Value : null;
    }

    public static IApplicationBuilder UseUnifiedStatusCodePages(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(async statusContext =>
        {
            var http = statusContext.HttpContext;
            var status = http.Response.StatusCode;

            var (code, message) = status switch
            {
                StatusCodes.Status401Unauthorized => (ErrorCodes.Unauthorized, "Authentication is required"),
                StatusCodes.Status403Forbidden => (ErrorCodes.Forbidden, "You do not have permission to perform this action"),
                StatusCodes.Status404NotFound when RequestedUnsupportedVersion(http.Request.Path) is { } version =>
                    (ErrorCodes.UnsupportedApiVersion, $"API version '{version}' is not supported. Supported versions: {string.Join(", ", SupportedVersions)}"),
                StatusCodes.Status404NotFound => (ErrorCodes.EndpointNotFound,
                    $"Endpoint {http.Request.Method} {http.Request.Path} not found"),
                StatusCodes.Status405MethodNotAllowed => (ErrorCodes.MethodNotAllowed,
                    $"Method {http.Request.Method} is not allowed for {http.Request.Path}"),
                StatusCodes.Status415UnsupportedMediaType => (ErrorCodes.UnsupportedMediaType,
                    "Content-Type must be application/json"),
                _ => ($"HTTP_{status}", "Request failed")
            };

            var result = ReturnResult<object>.Fail(status, code, message);
            result.TraceId = http.TraceIdentifier;

            http.Response.ContentType = "application/json; charset=utf-8";
            await http.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
        });
    }
}
