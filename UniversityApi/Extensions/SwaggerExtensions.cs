using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UniversityApi.Extensions;

/// <summary>
/// Swagger / OpenAPI: отдельный документ для каждой версии API (v1, v2) и поддержка JWT Bearer
/// (кнопка Authorize, замок у защищённых endpoints).
/// </summary>
public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";

    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "University Course Management API",
                Version = "v1",
                Description = "REST Web API для управления студентами, преподавателями, курсами и записями на курсы. " +
                              "Все ответы возвращаются в едином формате ReturnResult<T>.\n\n" +
                              "**Как авторизоваться:** выполните `POST /api/auth/login`, скопируйте `accessToken` " +
                              "и вставьте его в окно **Authorize** (без слова Bearer)."
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "University Course Management API",
                Version = "v2",
                Description = "Версия 2: расширенное представление курсов — сведения о преподавателе " +
                              "и количество записанных студентов. Остальные ресурсы доступны в v1."
            });

            // В документ версии попадают её endpoints; старые адреса без версии (/api/courses)
            // работают, но в документации не дублируются — там показаны /api/v1/courses.
            c.DocInclusionPredicate((docName, api) =>
                api.GroupName == docName &&
                !(api.RelativePath ?? string.Empty).StartsWith("api/courses", StringComparison.OrdinalIgnoreCase));

            // XML-комментарии (///) отображаются в Swagger как описания endpoints и примеры значений
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            // Порядок групп в Swagger
            var order = new[] { "Auth", "Users", "Roles", "Students", "Teachers", "Courses", "Enrollments", "Diagnostics" };
            c.OrderActionsBy(api =>
            {
                var controller = api.ActionDescriptor.RouteValues["controller"] ?? string.Empty;
                var index = Array.IndexOf(order, controller);
                return $"{(index < 0 ? 99 : index):D2}_{api.RelativePath}_{api.HttpMethod}";
            });

            // DateOnly передаётся строкой формата yyyy-MM-dd
            c.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string", Format = "date", Example = new OpenApiString("2004-05-15")
            });

            // JWT Bearer: кнопка Authorize
            c.AddSecurityDefinition(BearerScheme, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "JWT Access Token из POST /api/auth/login. Вставьте только сам токен, без слова Bearer."
            });

            c.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }

    public static WebApplication UseAppSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "University Course Management API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "University Course Management API v2");
            c.DocumentTitle = "University Course Management API";
            c.EnablePersistAuthorization(); // токен не теряется при обновлении страницы и смене версии
            c.DisplayRequestDuration();
        });

        return app;
    }
}

/// <summary>
/// Помечает в Swagger защищённые endpoints (замок, ответы 401/403) и пишет, какая политика нужна.
/// Открытые endpoints ([AllowAnonymous]) остаются без замка.
/// </summary>
public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any())
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = Array.Empty<string>()
            }
        };

        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized — токен не передан, просрочен, подделан или выдан другим издателем"
        });
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden — пользователь аутентифицирован, но у него нет прав на действие или ресурс"
        });

        var policies = metadata.OfType<IAuthorizeData>()
            .Select(a => a.Policy ?? (a.Roles is null ? null : $"роли {a.Roles}"))
            .Where(p => p is not null)
            .Distinct()
            .ToList();

        var access = policies.Count == 0 ? "любой аутентифицированный пользователь" : string.Join(", ", policies);
        operation.Description = $"🔒 Доступ: {access}.\n\n{operation.Description}".TrimEnd();
    }
}
