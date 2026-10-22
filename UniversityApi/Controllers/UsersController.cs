using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Users;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Управление пользователями: роли, связь со Student/Teacher, снятие блокировки. Только Admin.
/// Это единственный способ изменить роль — поэтому Student не может повысить себе привилегии.
/// </summary>
[Route("api/users")]
[Authorize(Policy = AppPolicies.AdminOnly)]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    /// <summary>Все пользователи с ролями (без хешей паролей и других секретов).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => ToActionResult(await _service.GetAllAsync());

    /// <summary>Пользователь по ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
        => ToActionResult(await _service.GetByIdAsync(id));

    /// <summary>Назначить пользователю роль (Admin, Teacher, Student).</summary>
    /// <remarks>
    /// Роль попадёт в JWT при следующем входе пользователя: уже выданный токен содержит старый набор ролей
    /// и действует до истечения срока (15 минут).
    /// </remarks>
    [HttpPost("{id}/roles")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRole(string id, [FromBody] AssignRoleDto dto)
        => ToActionResult(await _service.AddRoleAsync(id, dto));

    /// <summary>Снять роль с пользователя. Снять роль Admin с самого себя нельзя (409).</summary>
    [HttpDelete("{id}/roles/{role}")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveRole(string id, string role)
        => ToActionResult(await _service.RemoveRoleAsync(id, role));

    /// <summary>Связать учётную запись с существующим студентом и/или преподавателем.</summary>
    /// <remarks>
    /// Пример: { "studentId": 3, "teacherId": null }. Передайте null, чтобы убрать связь.
    /// Один студент/преподаватель может быть связан только с одной учётной записью (иначе 409).
    /// </remarks>
    [HttpPut("{id}/profile")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkProfile(string id, [FromBody] LinkProfileDto dto)
        => ToActionResult(await _service.LinkProfileAsync(id, dto));

    /// <summary>Снять блокировку после неудачных попыток входа.</summary>
    [HttpPost("{id}/unlock")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlock(string id)
        => ToActionResult(await _service.UnlockAsync(id));
}
