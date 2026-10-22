using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>
/// Записи студентов на курсы и оценки.
/// Просмотр: Admin — все, Teacher — по своим курсам, Student — только свои.
/// Оценки: Admin или преподаватель этого курса. Запись и отчисление — только Admin.
/// </summary>
[Route("api/enrollments")]
[Authorize]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    /// <summary>Получить список записей (можно отфильтровать по студенту и/или курсу).</summary>
    /// <remarks>
    /// Результат зависит от роли: Student получает только свои записи (studentId другого студента — 403),
    /// Teacher — только записи своих курсов (courseId чужого курса — 403).
    /// </remarks>
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
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
        => ToCreatedResult(await _service.EnrollAsync(dto), nameof(GetById), e => new { id = e.Id });

    /// <summary>Изменить оценку студента по курсу.</summary>
    /// <remarks>
    /// Политика GradeManagement пропускает Admin и Teacher; дополнительно проверяется,
    /// что курс принадлежит этому преподавателю (чужой курс — 403 NOT_COURSE_TEACHER).
    /// </remarks>
    [HttpPut("{id:int}/grade")]
    [Authorize(Policy = AppPolicies.GradeManagement)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] EnrollmentUpdateGradeDto dto)
        => ToActionResult(await _service.UpdateGradeAsync(id, dto));

    /// <summary>Удалить студента с курса (удалить запись).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await _service.DeleteAsync(id));
}
