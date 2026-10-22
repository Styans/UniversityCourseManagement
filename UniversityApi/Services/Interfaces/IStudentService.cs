using UniversityApi.Common;
using UniversityApi.DTO.Students;

namespace UniversityApi.Services.Interfaces;

public interface IStudentService
{
    Task<ReturnResult<IReadOnlyList<StudentDto>>> GetAllAsync(string? search);
    Task<ReturnResult<StudentDto>> GetByIdAsync(int id);
    Task<ReturnResult<StudentDetailsDto>> GetWithCoursesAsync(int id);
    Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto);
    Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto);
    Task<ReturnResult<StudentDto>> DeleteAsync(int id);
}
