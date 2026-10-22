using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;

namespace UniversityApi.Services.Interfaces;

public interface ICourseService
{
    // ---------- v1 ----------
    Task<ReturnResult<IReadOnlyList<CourseDto>>> GetAllAsync(CourseFilterDto filter);
    Task<ReturnResult<CourseDto>> GetByIdAsync(int id);

    // ---------- v2: те же фильтры и проверки, расширенный DTO ----------
    Task<ReturnResult<IReadOnlyList<CourseV2Dto>>> GetAllV2Async(CourseFilterDto filter);
    Task<ReturnResult<CourseV2Dto>> GetByIdV2Async(int id);

    Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto);
    Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto);
    Task<ReturnResult<CourseDto>> DeleteAsync(int id);
}
