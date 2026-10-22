namespace UniversityApi.Auth;

/// <summary>Роли пользователей. Создаются через RoleManager при запуске (Data/Seed/IdentitySeeder).</summary>
public static class AppRoles
{
    /// <summary>Управление пользователями и ролями, полный доступ к университетским данным.</summary>
    public const string Admin = "Admin";

    /// <summary>Просмотр курсов, изменение оценок только по собственным курсам.</summary>
    public const string Teacher = "Teacher";

    /// <summary>Просмотр курсов, собственных записей и оценок.</summary>
    public const string Student = "Student";

    public static readonly string[] All = { Admin, Teacher, Student };
}

/// <summary>Названия политик авторизации (регистрация — в AuthExtensions.AddAppAuthorization).</summary>
public static class AppPolicies
{
    /// <summary>Только Admin: изменение университетских данных, управление пользователями и ролями.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Admin или Teacher: просмотр списка студентов.</summary>
    public const string Staff = "Staff";

    /// <summary>
    /// Изменение оценок: Admin или Teacher, у которого учётная запись связана с сущностью Teacher
    /// (есть claim teacher_id). Принадлежность курса преподавателю проверяется отдельно — по ресурсу.
    /// </summary>
    public const string GradeManagement = "GradeManagement";
}

/// <summary>Собственные типы claims в JWT (короткие имена, без длинных URI из ClaimTypes).</summary>
public static class AppClaimTypes
{
    public const string UserId = "sub";
    public const string UserName = "name";
    public const string Role = "role";

    /// <summary>Id сущности Student, с которой связана учётная запись.</summary>
    public const string StudentId = "student_id";

    /// <summary>Id сущности Teacher, с которой связана учётная запись.</summary>
    public const string TeacherId = "teacher_id";
}
