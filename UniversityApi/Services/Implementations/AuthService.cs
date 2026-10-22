using Microsoft.AspNetCore.Identity;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;
using UniversityApi.Models;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

/// <summary>
/// Регистрация, вход и сведения о текущем пользователе.
/// Пароль проверяет Identity (SignInManager + хеш PBKDF2), JWT выдаёт ITokenService.
/// Пароли и токены в лог не пишутся — только имя пользователя и результат.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokens;
    private readonly ICurrentUser _currentUser;
    private readonly UserDtoFactory _userDtos;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        ITokenService tokens, ICurrentUser currentUser, UserDtoFactory userDtos, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokens = tokens;
        _currentUser = currentUser;
        _userDtos = userDtos;
        _logger = logger;
    }

    public async Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto)
    {
        var userName = dto.UserName.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        // Бизнес-правило: имя пользователя уникально (дополнительно — уникальный индекс NormalizedUserName в БД)
        if (await _userManager.FindByNameAsync(userName) is not null)
        {
            _logger.LogWarning("Registration rejected: user name {UserName} is already taken", userName);
            return ReturnResult<UserDto>.Conflict(ErrorCodes.UserNameAlreadyExists,
                $"User name '{userName}' is already taken");
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            _logger.LogWarning("Registration rejected: email {Email} is already used", email);
            return ReturnResult<UserDto>.Conflict(ErrorCodes.UserEmailAlreadyExists,
                $"User with email '{email}' already exists");
        }

        var user = new ApplicationUser { UserName = userName, Email = email };

        // Identity сам проверяет требования к паролю и сохраняет только его хеш
        var created = await _userManager.CreateAsync(user, dto.Password);
        if (!created.Succeeded)
            return IdentityErrors<UserDto>(created, userName);

        // Новый пользователь получает минимальную роль. Admin/Teacher назначает только администратор.
        await _userManager.AddToRoleAsync(user, AppRoles.Student);

        _logger.LogInformation("User registered: Id={UserId}, UserName={UserName}, Role={Role}",
            user.Id, user.UserName, AppRoles.Student);

        return ReturnResult<UserDto>.Created(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var userName = dto.UserName.Trim();
        var user = await _userManager.FindByNameAsync(userName);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user {UserName} not found", userName);
            return InvalidCredentials();
        }

        // lockoutOnFailure: true — после N неудачных попыток учётная запись блокируется на время
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Login rejected: account {UserName} is locked out until {LockoutEnd}",
                userName, user.LockoutEnd);
            return ReturnResult<AuthResponseDto>.Unauthorized(ErrorCodes.AccountLocked,
                "Account is temporarily locked due to multiple failed login attempts. Try again later");
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: wrong password for {UserName} (failed attempts: {Count})",
                userName, await _userManager.GetAccessFailedCountAsync(user));
            return InvalidCredentials();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokens.CreateAccessToken(user, roles);

        _logger.LogInformation("Login succeeded: UserName={UserName}, Roles={Roles}, ExpiresAt={ExpiresAt:u}",
            user.UserName, string.Join(",", roles), token.ExpiresAt);

        return ReturnResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = await _userDtos.CreateAsync(user)
        });
    }

    public async Task<ReturnResult<UserDto>> GetCurrentUserAsync()
    {
        var user = await FindCurrentUserAsync();
        if (user is null)
            return ReturnResult<UserDto>.NotFound(ErrorCodes.UserNotFound, "Current user no longer exists");

        return ReturnResult<UserDto>.Success(await _userDtos.CreateAsync(user));
    }

    public async Task<ReturnResult<AuthResponseDto>> CreateExpiredTokenForCurrentUserAsync()
    {
        var user = await FindCurrentUserAsync();
        if (user is null)
            return ReturnResult<AuthResponseDto>.NotFound(ErrorCodes.UserNotFound, "Current user no longer exists");

        var token = _tokens.CreateExpiredAccessToken(user, await _userManager.GetRolesAsync(user));
        _logger.LogInformation("Demo expired token issued for {UserName}", user.UserName);

        return ReturnResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = await _userDtos.CreateAsync(user)
        });
    }

    private async Task<ApplicationUser?> FindCurrentUserAsync()
        => _currentUser.UserId is { } id ? await _userManager.FindByIdAsync(id) : null;

    /// <summary>
    /// Одинаковый ответ для «нет такого пользователя» и «неверный пароль»,
    /// чтобы по ответу нельзя было узнать, существует ли учётная запись.
    /// </summary>
    private static ReturnResult<AuthResponseDto> InvalidCredentials()
        => ReturnResult<AuthResponseDto>.Unauthorized(ErrorCodes.InvalidCredentials, "Invalid user name or password");

    private ReturnResult<T> IdentityErrors<T>(IdentityResult result, string userName)
    {
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
            return ReturnResult<T>.Conflict(ErrorCodes.UserNameAlreadyExists, $"User name '{userName}' is already taken");

        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateEmail)))
            return ReturnResult<T>.Conflict(ErrorCodes.UserEmailAlreadyExists, "User with this email already exists");

        var errors = result.Errors
            .GroupBy(e => e.Code.StartsWith("Password", StringComparison.Ordinal) ? "password"
                : e.Code.Contains("Email", StringComparison.Ordinal) ? "email" : "userName")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

        _logger.LogWarning("Registration rejected for {UserName}: {Codes}",
            userName, string.Join(", ", result.Errors.Select(e => e.Code)));

        return ReturnResult<T>.BadRequest(ErrorCodes.ValidationError, "One or more validation errors occurred", errors);
    }
}
