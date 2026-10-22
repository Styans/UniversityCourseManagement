using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Auth;
using UniversityApi.Models;

namespace UniversityApi.Data.Seed;

/// <summary>
/// Создание ролей (RoleManager) и демонстрационных учётных записей (UserManager).
/// Роли создаются в любом окружении. Демо-пользователи — только если в конфигурации задан
/// Seed:DemoUsersPassword (есть в appsettings.Development.json; в Production его нет).
/// Пароли в лог не пишутся.
/// </summary>
public static class IdentitySeeder
{
    private sealed record DemoUser(string UserName, string Email, string Role, string? StudentEmail, string? TeacherEmail);

    /// <summary>
    /// Демо-аккаунты связаны с тестовыми данными из DbSeeder (по email студента/преподавателя):
    /// teacher.abenov ведёт «Web Services Development» и «Software Testing»,
    /// teacher.sadykova — «Databases»; student.bekova и student.omarov записаны на курсы с оценками.
    /// </summary>
    private static readonly DemoUser[] DemoUsers =
    {
        new("admin",            "admin@university.kz",                     AppRoles.Admin,   null, null),
        new("teacher.abenov",   "y.abenov@university.kz",                  AppRoles.Teacher, null, "y.abenov@university.kz"),
        new("teacher.sadykova", "g.sadykova@university.kz",                AppRoles.Teacher, null, "g.sadykova@university.kz"),
        new("student.bekova",   "aruzhan.bekova@student.university.kz",    AppRoles.Student, "aruzhan.bekova@student.university.kz", null),
        new("student.omarov",   "nursultan.omarov@student.university.kz",  AppRoles.Student, "nursultan.omarov@student.university.kz", null)
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        foreach (var role in AppRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
                continue;

            await roleManager.CreateAsync(new IdentityRole(role));
            logger.LogInformation("Role created: {Role}", role);
        }

        var demoPassword = configuration["Seed:DemoUsersPassword"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(demoPassword))
        {
            // Production: создаётся только администратор, если пароль передан через секреты / переменные окружения
            if (!string.IsNullOrEmpty(adminPassword))
                await EnsureUserAsync(userManager, db, DemoUsers[0], adminPassword, logger);
            return;
        }

        foreach (var demo in DemoUsers)
        {
            var password = demo.Role == AppRoles.Admin && !string.IsNullOrEmpty(adminPassword) ? adminPassword : demoPassword;
            await EnsureUserAsync(userManager, db, demo, password, logger);
        }
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext db,
        DemoUser demo, string password, ILogger logger)
    {
        if (await userManager.FindByNameAsync(demo.UserName) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = demo.UserName,
            Email = demo.Email,
            EmailConfirmed = true,
            StudentId = demo.StudentEmail is null
                ? null
                : await db.Students.Where(s => s.Email == demo.StudentEmail).Select(s => (int?)s.Id).FirstOrDefaultAsync(),
            TeacherId = demo.TeacherEmail is null
                ? null
                : await db.Teachers.Where(t => t.Email == demo.TeacherEmail).Select(t => (int?)t.Id).FirstOrDefaultAsync()
        };

        // Не связываем, если этот студент/преподаватель уже привязан к другой учётной записи
        if (user.StudentId is { } sid && await userManager.Users.AnyAsync(u => u.StudentId == sid))
            user.StudentId = null;
        if (user.TeacherId is { } tid && await userManager.Users.AnyAsync(u => u.TeacherId == tid))
            user.TeacherId = null;

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            logger.LogWarning("Demo user {UserName} was not created: {Errors}",
                demo.UserName, string.Join(", ", created.Errors.Select(e => e.Code)));
            return;
        }

        await userManager.AddToRoleAsync(user, demo.Role);
        logger.LogInformation("Demo user created: {UserName} ({Role}), StudentId={StudentId}, TeacherId={TeacherId}",
            user.UserName, demo.Role, user.StudentId, user.TeacherId);
    }
}
