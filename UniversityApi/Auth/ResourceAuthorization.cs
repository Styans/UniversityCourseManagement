using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace UniversityApi.Auth;

/// <summary>
/// Операции для resource-based авторизации.
/// Роль отвечает на вопрос «может ли Teacher вообще менять оценки»,
/// а проверка по ресурсу — «может ли ЭТОТ Teacher менять оценку в ЭТОМ курсе».
/// </summary>
public static class ResourceOperations
{
    public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
    public static readonly OperationAuthorizationRequirement UpdateGrade = new() { Name = nameof(UpdateGrade) };
}

/// <summary>Ресурс «профиль студента» (данные студента и его оценки).</summary>
public sealed record StudentResource(int StudentId);

/// <summary>Ресурс «запись на курс»: чья это запись и кто ведёт курс.</summary>
public sealed record EnrollmentResource(int StudentId, int CourseTeacherId);

/// <summary>
/// Доступ к профилю студента:
/// Admin и Teacher — читают любой профиль; Student — только свой (student_id в токене = id профиля).
/// </summary>
public class StudentAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, StudentResource>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, StudentResource resource)
    {
        var user = context.User;

        if (user.IsInRole(AppRoles.Admin))
        {
            context.Succeed(requirement);
        }
        else if (requirement.Name == ResourceOperations.Read.Name)
        {
            if (user.IsInRole(AppRoles.Teacher) ||
                (user.IsInRole(AppRoles.Student) && user.GetIntClaim(AppClaimTypes.StudentId) == resource.StudentId))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Доступ к записи на курс (и оценке в ней):
/// Admin — всё; Teacher — читать и менять оценку только в своих курсах;
/// Student — только читать свои записи.
/// </summary>
public class EnrollmentAuthorizationHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, EnrollmentResource>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement, EnrollmentResource resource)
    {
        var user = context.User;

        if (user.IsInRole(AppRoles.Admin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var isCourseTeacher = user.IsInRole(AppRoles.Teacher) &&
                              user.GetIntClaim(AppClaimTypes.TeacherId) == resource.CourseTeacherId;

        var isOwner = user.IsInRole(AppRoles.Student) &&
                      user.GetIntClaim(AppClaimTypes.StudentId) == resource.StudentId;

        if (requirement.Name == ResourceOperations.Read.Name && (isCourseTeacher || isOwner))
            context.Succeed(requirement);

        if (requirement.Name == ResourceOperations.UpdateGrade.Name && isCourseTeacher)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
