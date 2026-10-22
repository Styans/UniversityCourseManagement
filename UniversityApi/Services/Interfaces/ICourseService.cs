using UniversityApi.Common;
using UniversityApi.DTO.Courses;

namespace UniversityApi.Services.Interfaces;

public interface ICourseService
{
    Task<ReturnResult<IReadOnlyList<CourseDto>>> GetAllAsync(CourseFilterDto filter);
    Task<ReturnResult<CourseDto>> GetByIdAsync(int id);
    Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto);
    Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto);
    Task<ReturnResult<CourseDto>> DeleteAsync(int id);
}
