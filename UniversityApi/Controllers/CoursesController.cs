using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

/// <summary>Учебные курсы: CRUD и фильтрация.</summary>
[Route("api/courses")]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    /// <summary>Получить курсы с необязательной фильтрацией.</summary>
    /// <remarks>
    /// Примеры:
    /// GET /api/courses?minCredits=5
    /// GET /api/courses?teacherId=3
    /// GET /api/courses?search=data&amp;department=Computer
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IReadOnlyList<CourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter)
        => ToActionResult(await _service.GetAllAsync(filter));

    /// <summary>Получить курс по ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToActionResult(await _service.GetByIdAsync(id));

    /// <summary>Создать курс и закрепить за преподавателем.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
        => ToCreatedResult(await _service.CreateAsync(dto), nameof(GetById), c => new { id = c.Id });

    /// <summary>Изменить курс (в том числе сменить преподавателя).</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
        => ToActionResult(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить курс (записи на него удаляются каскадно).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
        => ToActionResult(await _service.DeleteAsync(id));
}
