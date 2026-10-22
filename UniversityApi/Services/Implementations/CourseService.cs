using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ICourseRepository courses, ITeacherRepository teachers, IMapper mapper,
        ILogger<CourseService> logger)
    {
        _courses = courses;
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    // Чтение курсов для v1 и v2 идёт через общие методы LoadFilteredAsync / LoadByIdAsync:
    // фильтрация, поиск и «404 если нет» реализованы один раз, версии отличаются только DTO.

    public async Task<ReturnResult<IReadOnlyList<CourseDto>>> GetAllAsync(CourseFilterDto filter)
    {
        var courses = await LoadFilteredAsync(filter);
        return ReturnResult<IReadOnlyList<CourseDto>>.Success(_mapper.Map<List<CourseDto>>(courses));
    }

    public async Task<ReturnResult<CourseDto>> GetByIdAsync(int id)
    {
        var course = await LoadByIdAsync(id);
        if (course is null)
            return CourseNotFound<CourseDto>(id);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<IReadOnlyList<CourseV2Dto>>> GetAllV2Async(CourseFilterDto filter)
    {
        var courses = await LoadFilteredAsync(filter);
        var result = await ToV2Async(courses);
        return ReturnResult<IReadOnlyList<CourseV2Dto>>.Success(result);
    }

    public async Task<ReturnResult<CourseV2Dto>> GetByIdV2Async(int id)
    {
        var course = await LoadByIdAsync(id);
        if (course is null)
            return CourseNotFound<CourseV2Dto>(id);

        var result = await ToV2Async(new[] { course });
        return ReturnResult<CourseV2Dto>.Success(result[0]);
    }

    private Task<IReadOnlyList<Course>> LoadFilteredAsync(CourseFilterDto filter)
        => _courses.GetFilteredAsync(filter.Search, filter.TeacherId, filter.Department, filter.MinCredits, filter.MaxCredits);

    private Task<Course?> LoadByIdAsync(int id) => _courses.GetWithTeacherAsync(id);

    /// <summary>Маппинг в DTO v2 + количество записанных студентов (одним запросом на все курсы).</summary>
    private async Task<List<CourseV2Dto>> ToV2Async(IReadOnlyList<Course> courses)
    {
        var counts = await _courses.GetEnrollmentCountsAsync(courses.Select(c => c.Id).ToList());
        var result = _mapper.Map<List<CourseV2Dto>>(courses);

        foreach (var dto in result)
            dto.EnrolledStudentsCount = counts.TryGetValue(dto.Id, out var count) ? count : 0;

        return result;
    }

    public async Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto)
    {
        // Бизнес-правило №4: курс не может ссылаться на несуществующего преподавателя
        var teacherId = dto.TeacherId!.Value;
        if (!await _teachers.ExistsAsync(teacherId))
            return TeacherNotFound(teacherId, "Course was not created");

        var course = _mapper.Map<Course>(dto);
        await _courses.AddAsync(course);

        _logger.LogInformation("Course created: Id={CourseId}, Name={CourseName}, Credits={Credits}, TeacherId={TeacherId}",
            course.Id, course.Name, course.Credits, course.TeacherId);

        var created = await _courses.GetWithTeacherAsync(course.Id);
        return ReturnResult<CourseDto>.Created(_mapper.Map<CourseDto>(created));
    }

    public async Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course is null)
            return CourseNotFound<CourseDto>(id);

        var teacherId = dto.TeacherId!.Value;
        if (!await _teachers.ExistsAsync(teacherId))
            return TeacherNotFound(teacherId, $"Course {id} was not updated");

        _mapper.Map(dto, course);
        await _courses.UpdateAsync(course);

        _logger.LogInformation("Course updated: Id={CourseId}, Name={CourseName}, Credits={Credits}, TeacherId={TeacherId}",
            course.Id, course.Name, course.Credits, course.TeacherId);

        var updated = await _courses.GetWithTeacherAsync(course.Id);
        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(updated));
    }

    public async Task<ReturnResult<CourseDto>> DeleteAsync(int id)
    {
        var course = await _courses.GetWithTeacherAsync(id);
        if (course is null)
            return CourseNotFound<CourseDto>(id);

        var tracked = await _courses.GetByIdAsync(id);
        await _courses.DeleteAsync(tracked!);

        _logger.LogInformation("Course deleted: Id={CourseId}, Name={CourseName} (enrollments removed by cascade)",
            course.Id, course.Name);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    private ReturnResult<T> CourseNotFound<T>(int id)
    {
        _logger.LogWarning("Course not found: Id={CourseId}", id);
        return ReturnResult<T>.NotFound(ErrorCodes.CourseNotFound, $"Course with id {id} not found");
    }

    private ReturnResult<CourseDto> TeacherNotFound(int teacherId, string action)
    {
        _logger.LogWarning("{Action}: teacher {TeacherId} does not exist", action, teacherId);
        return ReturnResult<CourseDto>.NotFound(ErrorCodes.TeacherNotFound, $"Teacher with id {teacherId} not found");
    }
}
