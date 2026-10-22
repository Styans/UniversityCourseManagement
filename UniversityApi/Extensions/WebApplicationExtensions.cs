using System.Text.Json;
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
            await db.Database.EnsureCreatedAsync();
        }

        if (app.Configuration.GetValue("Database:SeedTestData", true))
            await DbSeeder.SeedAsync(db, logger);
    }

    /// <summary>
    /// Ответы без тела с кодами 404/405/415 (несуществующий URL, неверный метод, не тот Content-Type)
    /// тоже приводятся к единому формату ReturnResult.
    /// </summary>
    public static IApplicationBuilder UseUnifiedStatusCodePages(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(async statusContext =>
        {
            var http = statusContext.HttpContext;
            var status = http.Response.StatusCode;

            var (code, message) = status switch
            {
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
