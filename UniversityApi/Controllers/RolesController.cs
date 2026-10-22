using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Users;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Роли (RoleManager). Только Admin.</summary>
[Route("api/roles")]
[Authorize(Policy = AppPolicies.AdminOnly)]
public class RolesController : ApiControllerBase
{
    private readonly IUserService _service;

    public RolesController(IUserService service)
    {
        _service = service;
    }

    /// <summary>Список ролей.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => ToActionResult(await _service.GetRolesAsync());

    /// <summary>Создать роль.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<string>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        => ToActionResult(await _service.CreateRoleAsync(dto));
}
