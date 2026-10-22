using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Записи студентов на курсы.</summary>
[Route("api/enrollments")]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    /// <summary>Получить список записей (можно отфильтровать по студенту и/или курсу).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<EnrollmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] EnrollmentFilterDto filter)
        => ToActionResult(await _service.GetAllAsync(filter));

    /// <summary>Получить запись по ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await _service.GetByIdAsync(id));

    /// <summary>Записать студента на курс.</summary>
    /// <remarks>
    /// 404 — студент или курс не существует; 409 — студент уже записан на этот курс.
    /// </remarks>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
        => ToCreatedResult(await _service.EnrollAsync(dto), nameof(GetById), e => new { id = e.Id });

    /// <summary>Изменить оценку студента по курсу.</summary>
    [HttpPut("{id:int}/grade")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] EnrollmentUpdateGradeDto dto)
        => ToActionResult(await _service.UpdateGradeAsync(id, dto));

    /// <summary>Удалить студента с курса (удалить запись).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await _service.DeleteAsync(id));
}
