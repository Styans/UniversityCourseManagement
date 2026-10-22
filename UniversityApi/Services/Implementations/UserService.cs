using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Users;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

/// <summary>
/// Управление пользователями и ролями (доступно только Admin — проверяется политикой AdminOnly на контроллере).
/// Работает через UserManager и RoleManager из ASP.NET Core Identity.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IStudentRepository _students;
    private readonly ITeacherRepository _teachers;
    private readonly ICurrentUser _currentUser;
    private readonly UserDtoFactory _userDtos;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        IStudentRepository students, ITeacherRepository teachers, ICurrentUser currentUser,
        UserDtoFactory userDtos, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _students = students;
        _teachers = teachers;
        _currentUser = currentUser;
        _userDtos = userDtos;
        _logger = logger;
    }

    public async Task<ReturnResult<IReadOnlyList<UserDto>>> GetAllAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();

        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
            result.Add(await _userDtos.CreateAsync(user));

        return ReturnResult<IReadOnlyList<UserDto>>.Success(result);
    }

    public async Task<ReturnResult<UserDto>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return UserNotFound(id);

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<UserDto>> AddRoleAsync(string id, AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return UserNotFound(id);

        var role = await _roleManager.FindByNameAsync(dto.Role.Trim());
        if (role?.Name is null)
            return RoleNotFound(dto.Role);

        if (!await _userManager.IsInRoleAsync(user, role.Name))
        {
            var added = await _userManager.AddToRoleAsync(user, role.Name);
            if (!added.Succeeded)
                throw new InvalidOperationException($"Role {role.Name} was not added: {string.Join(", ", added.Errors.Select(e => e.Code))}");

            // Смена SecurityStamp фиксирует изменение прав учётной записи
            await _userManager.UpdateSecurityStampAsync(user);
        }

        _logger.LogInformation("Role granted: Admin {AdminName} added role {Role} to user {UserName} (Id={UserId})",
            _currentUser.UserName, role.Name, user.UserName, user.Id);

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<UserDto>> RemoveRoleAsync(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return UserNotFound(id);

        var identityRole = await _roleManager.FindByNameAsync(role.Trim());
        if (identityRole?.Name is null)
            return RoleNotFound(role);

        // Защита от потери доступа: администратор не может снять роль Admin сам с себя
        if (identityRole.Name == AppRoles.Admin && user.Id == _currentUser.UserId)
        {
            _logger.LogWarning("Admin {AdminName} tried to remove own Admin role", _currentUser.UserName);
            return ReturnResult<UserDto>.Conflict(ErrorCodes.CannotRemoveOwnAdminRole,
                "You cannot remove the Admin role from your own account");
        }

        if (await _userManager.IsInRoleAsync(user, identityRole.Name))
        {
            await _userManager.RemoveFromRoleAsync(user, identityRole.Name);
            await _userManager.UpdateSecurityStampAsync(user);
        }

        _logger.LogInformation("Role revoked: Admin {AdminName} removed role {Role} from user {UserName} (Id={UserId})",
            _currentUser.UserName, identityRole.Name, user.UserName, user.Id);

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<UserDto>> LinkProfileAsync(string id, LinkProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return UserNotFound(id);

        if (dto.StudentId is { } studentId)
        {
            if (!await _students.ExistsAsync(studentId))
                return ReturnResult<UserDto>.NotFound(ErrorCodes.StudentNotFound, $"Student with id {studentId} not found");

            if (await _userManager.Users.AnyAsync(u => u.StudentId == studentId && u.Id != user.Id))
                return ProfileAlreadyLinked("Student", studentId);
        }

        if (dto.TeacherId is { } teacherId)
        {
            if (!await _teachers.ExistsAsync(teacherId))
                return ReturnResult<UserDto>.NotFound(ErrorCodes.TeacherNotFound, $"Teacher with id {teacherId} not found");

            if (await _userManager.Users.AnyAsync(u => u.TeacherId == teacherId && u.Id != user.Id))
                return ProfileAlreadyLinked("Teacher", teacherId);
        }

        user.StudentId = dto.StudentId;
        user.TeacherId = dto.TeacherId;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        _logger.LogInformation("Profile linked: user {UserName} (Id={UserId}) -> StudentId={StudentId}, TeacherId={TeacherId}",
            user.UserName, user.Id, user.StudentId, user.TeacherId);

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<UserDto>> UnlockAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return UserNotFound(id);

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        _logger.LogInformation("User unlocked: {UserName} (Id={UserId}) by {AdminName}",
            user.UserName, user.Id, _currentUser.UserName);

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<IReadOnlyList<string>>> GetRolesAsync()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();
        return ReturnResult<IReadOnlyList<string>>.Success(roles);
    }

    public async Task<ReturnResult<string>> CreateRoleAsync(CreateRoleDto dto)
    {
        var name = dto.Name.Trim();
        if (await _roleManager.RoleExistsAsync(name))
            return ReturnResult<string>.Conflict(ErrorCodes.RoleAlreadyExists, $"Role '{name}' already exists");

        var created = await _roleManager.CreateAsync(new IdentityRole(name));
        if (!created.Succeeded)
            throw new InvalidOperationException($"Role {name} was not created: {string.Join(", ", created.Errors.Select(e => e.Code))}");

        _logger.LogInformation("Role created: {Role} by {AdminName}", name, _currentUser.UserName);
        return ReturnResult<string>.Created(name);
    }

    private ReturnResult<UserDto> UserNotFound(string id)
    {
        _logger.LogWarning("User not found: Id={UserId}", id);
        return ReturnResult<UserDto>.NotFound(ErrorCodes.UserNotFound, $"User with id {id} not found");
    }

    private ReturnResult<UserDto> RoleNotFound(string role)
    {
        _logger.LogWarning("Role not found: {Role}", role);
        return ReturnResult<UserDto>.NotFound(ErrorCodes.RoleNotFound,
            $"Role '{role}' not found. Available roles: {string.Join(", ", AppRoles.All)}");
    }

    private ReturnResult<UserDto> ProfileAlreadyLinked(string entity, int entityId)
    {
        _logger.LogWarning("{Entity} {EntityId} is already linked to another account", entity, entityId);
        return ReturnResult<UserDto>.Conflict(ErrorCodes.ProfileAlreadyLinked,
            $"{entity} with id {entityId} is already linked to another user account");
    }
}
