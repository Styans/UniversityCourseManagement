using UniversityApi.Common;
using UniversityApi.DTO.Teachers;

namespace UniversityApi.Services.Interfaces;

public interface ITeacherService
{
    Task<ReturnResult<IReadOnlyList<TeacherDto>>> GetAllAsync(string? department);
    Task<ReturnResult<TeacherDto>> GetByIdAsync(int id);
    Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto);
    Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto);
    Task<ReturnResult<TeacherDto>> DeleteAsync(int id);
}
