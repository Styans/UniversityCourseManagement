using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;

namespace UniversityApi.Services.Interfaces;

public interface IAuthService
{
    Task<ReturnResult<UserDto>> RegisterAsync(RegisterDto dto);
    Task<ReturnResult<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ReturnResult<UserDto>> GetCurrentUserAsync();

    /// <summary>Только для демонстрации сценария «просроченный JWT» (Development).</summary>
    Task<ReturnResult<AuthResponseDto>> CreateExpiredTokenForCurrentUserAsync();
}
