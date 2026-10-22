using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Студенты: CRUD и карточка студента с курсами.
/// Данные студентов (email, дата рождения, оценки) — персональные, поэтому анонимного доступа нет:
/// список — Admin и Teacher; профиль и оценки — Admin, Teacher или сам студент; изменения — только Admin.
/// </summary>
[Route("api/students")]
[Authorize]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    /// <summary>Получить всех студентов.</summary>
    /// <param name="search">Необязательный поиск по имени, фамилии или email.</param>
    [HttpGet]
    [Authorize(Policy = AppPolicies.Staff)]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
        => ToActionResult(await _service.GetAllAsync(search));

    /// <summary>Получить студента по ID. Student — только свой профиль (иначе 403).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await _service.GetByIdAsync(id));

    /// <summary>Получить студента вместе с курсами и оценками. Student — только свои оценки (иначе 403).</summary>
    [HttpGet("{id:int}/courses")]
    [ProducesResponseType(typeof(ReturnResult<StudentDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithCourses(int id)
        => ToActionResult(await _service.GetWithCoursesAsync(id));

    /// <summary>Создать студента.</summary>
    [HttpPost]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] StudentCreateDto dto)
        => ToCreatedResult(await _service.CreateAsync(dto), nameof(GetById), s => new { id = s.Id });

    /// <summary>Изменить студента.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
        => ToActionResult(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить студента (его записи на курсы удаляются каскадно).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await _service.DeleteAsync(id));
}
