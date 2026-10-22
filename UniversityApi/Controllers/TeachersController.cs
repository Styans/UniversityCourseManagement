using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Преподаватели: CRUD. Список и карточка преподавателя открыты (как справочник кафедры),
/// создание, изменение и удаление — только Admin.
/// </summary>
[Route("api/teachers")]
[Authorize(Policy = AppPolicies.AdminOnly)]
public class TeachersController : ApiControllerBase
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service)
    {
        _service = service;
    }

    /// <summary>Получить всех преподавателей.</summary>
    /// <param name="department">Необязательный фильтр по кафедре.</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? department)
        => ToActionResult(await _service.GetAllAsync(department));

    /// <summary>Получить преподавателя по ID.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await _service.GetByIdAsync(id));

    /// <summary>Создать преподавателя.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TeacherCreateDto dto)
        => ToCreatedResult(await _service.CreateAsync(dto), nameof(GetById), t => new { id = t.Id });

    /// <summary>Изменить преподавателя.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
        => ToActionResult(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить преподавателя. Нельзя, если за ним закреплены курсы (409).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await _service.DeleteAsync(id));
}
