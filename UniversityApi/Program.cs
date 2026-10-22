using System.Reflection;
using Microsoft.OpenApi.Models;
using UniversityApi.Common.Logging;
using UniversityApi.Extensions;
using UniversityApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---------- Логирование: консоль + файл logs/app-ГГГГММДД.log ----------
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "HH:mm:ss ";
});
builder.Logging.AddFile(
    Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Logging:File:Directory"] ?? "logs"),
    LogLevel.Information);

// ---------- Dependency Injection ----------
builder.Services.AddDatabase(builder.Configuration);              // DbContext (Scoped)
builder.Services.AddApplicationServices(builder.Configuration);   // Repositories, Services (Scoped), AutoMapper

builder.Services.AddControllers()
    .ConfigureValidationResponse();                                // 400 VALIDATION_ERROR в формате ReturnResult

// ---------- Swagger / OpenAPI ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University Course Management API",
        Version = "v1",
        Description = "REST Web API для управления студентами, преподавателями, курсами и записями на курсы. " +
                      "Все ответы возвращаются в едином формате ReturnResult<T>."
    });

    // XML-комментарии (///) отображаются в Swagger как описания endpoints и примеры значений
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);

    // Порядок групп в Swagger: Students → Teachers → Courses → Enrollments → Diagnostics
    var order = new[] { "Students", "Teachers", "Courses", "Enrollments", "Diagnostics" };
    c.OrderActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"] ?? string.Empty;
        var index = Array.IndexOf(order, controller);
        return $"{(index < 0 ? 99 : index):D2}_{api.RelativePath}_{api.HttpMethod}";
    });

    // DateOnly передаётся строкой формата yyyy-MM-dd
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date", Example = new Microsoft.OpenApi.Any.OpenApiString("2004-05-15") });
});

var app = builder.Build();

// ---------- HTTP pipeline (порядок важен) ----------
app.UseMiddleware<ExceptionHandlingMiddleware>(); // первым — чтобы ловить ошибки всех следующих звеньев
app.UseUnifiedStatusCodePages();                  // 404/405/415 без тела → ReturnResult

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "University Course Management API v1");
    c.DocumentTitle = "University Course Management API";
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

await app.InitializeDatabaseAsync();              // миграции + тестовые данные

app.Run();
