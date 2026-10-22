using Microsoft.Extensions.Options;
using UniversityApi.Auth;
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

// ---------- Безопасность ----------
builder.Services.AddAppIdentity();                                // ASP.NET Core Identity (пользователи, роли, хеши паролей)
builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment); // JWT Bearer
builder.Services.AddAppAuthorization();                           // роли + политики + проверка по ресурсу
builder.Services.AddAppCors(builder.Configuration);               // CORS для двух клиентских приложений

builder.Services.AddControllers()
    .ConfigureValidationResponse();                                // 400 VALIDATION_ERROR в формате ReturnResult

builder.Services.AddAppApiVersioning();                           // /api/v1/..., /api/v2/...
builder.Services.AddAppSwagger();                                 // Swagger: документы v1 и v2 + JWT

var app = builder.Build();

// ---------- HTTP pipeline (порядок важен) ----------
app.UseMiddleware<ExceptionHandlingMiddleware>(); // первым — чтобы ловить ошибки всех следующих звеньев
app.UseUnifiedStatusCodePages();                  // 401/403/404/405/415 без тела → ReturnResult

app.UseAppSwagger();

app.UseRouting();                                 // 1. выбирается endpoint (контроллер/action)
app.UseCors(SecurityExtensions.CorsPolicyName);   // 2. CORS — после Routing и до аутентификации (preflight OPTIONS без токена)
app.UseAuthentication();                          // 3. кто пользователь: проверка JWT → HttpContext.User (claims)
app.UseAuthorization();                           // 4. что ему можно: [Authorize], роли, политики

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription().AllowAnonymous();
// «Закрыто по умолчанию»: все контроллеры требуют аутентификации, если action не помечен [AllowAnonymous].
// (Не FallbackPolicy: она срабатывала бы и для несуществующих адресов, и вместо 404 был бы 401.)
app.MapControllers().RequireAuthorization();

// Проверка настроек JWT до старта: в Production без ключа подписи приложение не запустится
_ = app.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

await app.InitializeDatabaseAsync();              // миграции + тестовые данные + роли и демо-пользователи

if (SecurityExtensions.DevKeyGenerated)
{
    app.Logger.LogWarning(
        "Jwt:Key is not set in User Secrets — a temporary random signing key was generated. " +
        "Tokens will become invalid after restart. Run scripts/setup-dev-secrets.ps1 to set a permanent key");
}

app.Run();
