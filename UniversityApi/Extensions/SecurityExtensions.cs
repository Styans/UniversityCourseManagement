using System.Security.Cryptography;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Models;

namespace UniversityApi.Extensions;

/// <summary>
/// Регистрация механизмов безопасности: Identity, JWT Bearer, политики авторизации, CORS, версии API.
/// </summary>
public static class SecurityExtensions
{
    public const string CorsPolicyName = "UniversityClients";

    /// <summary>
    /// ASP.NET Core Identity поверх EF Core: учётные записи и роли хранятся в той же БД (таблицы AspNet*).
    /// AddIdentityCore (а не AddIdentity) — без cookie-аутентификации: API работает только с JWT.
    /// </summary>
    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Имя пользователя и email уникальны
                options.User.RequireUniqueEmail = true;

                // Минимальные требования к сложности пароля
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 4;

                // Блокировка после 5 неудачных попыток входа подряд на 5 минут
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        return services;
    }

    /// <summary>
    /// JWT Bearer: проверка подписи, издателя, аудитории и срока действия токена.
    /// Ответы 401/403 формируются в едином формате ReturnResult.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .PostConfigure(settings =>
            {
                // Только для Development: если ключ не задан в User Secrets, генерируется случайный ключ
                // на время работы процесса (после перезапуска все выданные токены станут недействительны).
                if (environment.IsDevelopment() && !settings.IsKeyValid())
                {
                    settings.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                    DevKeyGenerated = true;
                }
            })
            .Validate(s => s.IsKeyValid(),
                "Jwt:Key is not configured or shorter than 32 bytes. " +
                "Development: dotnet user-secrets set \"Jwt:Key\" \"<random string>\" --project UniversityApi; " +
                "Production: environment variable Jwt__Key")
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer) && !string.IsNullOrWhiteSpace(s.Audience),
                "Jwt:Issuer and Jwt:Audience are required")
            .Validate(s => s.AccessTokenMinutes is > 0 and <= 120, "Jwt:AccessTokenMinutes must be between 1 and 120")
            .ValidateOnStart();

        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Параметры проверки токена берутся из JwtSettings (IOptions), а не из захардкоженных строк
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                // Оставляем короткие имена claims (sub, name, role) как в токене
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,          // подпись: токен не подделан и не изменён
                    IssuerSigningKey = jwt.GetSigningKey(),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },

                    ValidateIssuer = true,                    // кто выдал
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,                  // для кого выдан
                    ValidAudience = jwt.Audience,

                    ValidateLifetime = true,                  // не просрочен
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,                // без «запаса» 5 минут по умолчанию

                    NameClaimType = AppClaimTypes.UserName,
                    RoleClaimType = AppClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = WriteUnauthorizedAsync,
                    OnForbidden = WriteForbiddenAsync
                };
            });

        return services;
    }

    /// <summary>Признак, что в Development ключ JWT сгенерирован автоматически (для предупреждения в логе).</summary>
    public static bool DevKeyGenerated { get; private set; }

    /// <summary>
    /// Политики авторизации. «Закрыто по умолчанию» включается в Program.cs:
    /// MapControllers().RequireAuthorization() — любой action без атрибутов требует аутентификации,
    /// открытые endpoints явно помечаются [AllowAnonymous].
    /// </summary>
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicies.AdminOnly, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(AppRoles.Admin))
            .AddPolicy(AppPolicies.Staff, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(AppRoles.Admin, AppRoles.Teacher))
            .AddPolicy(AppPolicies.GradeManagement, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(AppRoles.Admin) ||
                    (context.User.IsInRole(AppRoles.Teacher) && context.User.HasClaim(c => c.Type == AppClaimTypes.TeacherId))));

        // Обработчики проверки по ресурсу (принадлежность записи / профиля пользователю)
        services.AddSingleton<IAuthorizationHandler, StudentAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EnrollmentAuthorizationHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    /// <summary>
    /// CORS: именованная политика, разрешающая запросы из браузера только с конкретных адресов клиентов
    /// (список — в appsettings.json, секция Cors:AllowedOrigins).
    /// </summary>
    public static IServiceCollection AddAppCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(origins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Authorization", "Content-Type", "Accept")
            .WithExposedHeaders("Location", "api-supported-versions", "api-deprecated-versions")));

        return services;
    }

    /// <summary>
    /// Версионирование: версия задаётся сегментом URL (/api/v1/..., /api/v2/...).
    /// Запросы без версии (/api/students, /api/courses) считаются v1 — старые клиенты продолжают работать.
    /// </summary>
    public static IServiceCollection AddAppApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true; // заголовок api-supported-versions в ответе
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";          // группы Swagger: v1, v2
                options.SubstituteApiVersionInUrl = true;  // {version} → 1 / 2 в документации
            });

        return services;
    }

    private static Task WriteUnauthorizedAsync(JwtBearerChallengeContext context)
    {
        // Отменяем стандартный пустой ответ 401 и отдаём ReturnResult
        context.HandleResponse();

        var (code, message) = context.AuthenticateFailure switch
        {
            SecurityTokenExpiredException => (ErrorCodes.TokenExpired, "Access token has expired. Log in again"),
            not null => (ErrorCodes.InvalidToken, "Access token is invalid"),
            _ => (ErrorCodes.Unauthorized, "Authentication is required. Send the header: Authorization: Bearer <token>")
        };

        context.Response.Headers.WWWAuthenticate = context.AuthenticateFailure is null
            ? "Bearer"
            : "Bearer error=\"invalid_token\"";

        // В лог — только тип ошибки, сам токен никогда не логируется
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("UniversityApi.Authentication");
        logger.LogWarning("401 {Code} for {Method} {Path}{Reason}",
            code, context.Request.Method, context.Request.Path,
            context.AuthenticateFailure is null ? string.Empty : $" ({context.AuthenticateFailure.GetType().Name})");

        return context.HttpContext.WriteReturnResultAsync(ReturnResult<object>.Unauthorized(code, message));
    }

    private static Task WriteForbiddenAsync(ForbiddenContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("UniversityApi.Authorization");
        logger.LogWarning("403 FORBIDDEN: user {UserName} has no access to {Method} {Path}",
            context.HttpContext.User.Identity?.Name, context.Request.Method, context.Request.Path);

        return context.HttpContext.WriteReturnResultAsync(ReturnResult<object>.Forbidden(ErrorCodes.Forbidden,
            "You do not have permission to perform this action"));
    }
}
