using UniversityApi.Common;
using UniversityApi.DTO.Users;

namespace UniversityApi.Services.Interfaces;

public interface IUserService
{
    Task<ReturnResult<IReadOnlyList<UserDto>>> GetAllAsync();
    Task<ReturnResult<UserDto>> GetByIdAsync(string id);
    Task<ReturnResult<UserDto>> AddRoleAsync(string id, AssignRoleDto dto);
    Task<ReturnResult<UserDto>> RemoveRoleAsync(string id, string role);
    Task<ReturnResult<UserDto>> LinkProfileAsync(string id, LinkProfileDto dto);
    Task<ReturnResult<UserDto>> UnlockAsync(string id);
    Task<ReturnResult<IReadOnlyList<string>>> GetRolesAsync();
    Task<ReturnResult<string>> CreateRoleAsync(CreateRoleDto dto);
}
