using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers.V2;

/// <summary>
/// Учебные курсы, версия 2: расширенное представление — преподаватель (с кафедрой)
/// и количество записанных студентов. Фильтры те же, что в v1.
/// Бизнес-логика общая с v1 (ICourseService), отличается только DTO ответа.
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[AllowAnonymous]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    /// <summary>Получить курсы (v2) с необязательной фильтрацией.</summary>
    /// <remarks>
    /// Пример: GET /api/v2/courses?department=Software&amp;minCredits=4
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<CourseV2Dto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter)
        => ToActionResult(await _service.GetAllV2Async(filter));

    /// <summary>Получить курс по ID (v2).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<CourseV2Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await _service.GetByIdV2Async(id));
}
