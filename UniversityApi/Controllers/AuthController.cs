using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Auth;
using UniversityApi.DTO.Users;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Аутентификация: регистрация, вход (выдача JWT), сведения о текущем пользователе.
/// Не зависит от версии API — доступен и в документе v1, и в v2.
/// </summary>
[ApiVersionNeutral]
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>Регистрация нового пользователя (получает роль Student).</summary>
    /// <remarks>
    /// Пароль: минимум 8 символов, цифра, строчная и заглавная латинская буква.
    /// Имя пользователя и email должны быть уникальны (иначе 409).
    /// Административные права при регистрации не выдаются — роль назначает Admin.
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _service.RegisterAsync(dto);
        result.TraceId = HttpContext.TraceIdentifier;
        return result.IsSuccess ? CreatedAtAction(nameof(Me), null, result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>Вход: проверка пароля через Identity и выдача JWT Access Token.</summary>
    /// <remarks>
    /// Демо-пользователи (Development), пароль Demo#2026:
    /// admin · teacher.abenov · teacher.sadykova · student.bekova · student.omarov.
    /// После 5 неверных паролей подряд учётная запись блокируется на 5 минут.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
        => ToActionResult(await _service.LoginAsync(dto));

    /// <summary>Сведения о текущем пользователе (по JWT): роли и связь со Student/Teacher.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ReturnResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
        => ToActionResult(await _service.GetCurrentUserAsync());
}
