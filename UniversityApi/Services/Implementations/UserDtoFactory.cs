using Microsoft.AspNetCore.Identity;
using UniversityApi.DTO.Users;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Services.Implementations;

/// <summary>
/// Собирает UserDto из ApplicationUser: роли (через UserManager) и имена связанных Student/Teacher.
/// Используется и в AuthService (/api/auth/me), и в UserService (управление пользователями).
/// </summary>
public class UserDtoFactory
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRepository _students;
    private readonly ITeacherRepository _teachers;

    public UserDtoFactory(UserManager<ApplicationUser> userManager, IStudentRepository students,
        ITeacherRepository teachers)
    {
        _userManager = userManager;
        _students = students;
        _teachers = teachers;
    }

    public async Task<UserDto> CreateAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        string? studentName = null;
        if (user.StudentId.HasValue)
        {
            var student = await _students.GetByIdAsync(user.StudentId.Value);
            studentName = student is null ? null : $"{student.FirstName} {student.LastName}";
        }

        string? teacherName = null;
        if (user.TeacherId.HasValue)
        {
            var teacher = await _teachers.GetByIdAsync(user.TeacherId.Value);
            teacherName = teacher is null ? null : $"{teacher.FirstName} {teacher.LastName}";
        }

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.OrderBy(r => r).ToList(),
            StudentId = user.StudentId,
            StudentFullName = studentName,
            TeacherId = user.TeacherId,
            TeacherFullName = teacherName,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
            CreatedAt = user.CreatedAt
        };
    }
}
