using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;

namespace UniversityApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ReturnResult<IReadOnlyList<EnrollmentDto>>> GetAllAsync(EnrollmentFilterDto filter);
    Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id);
    Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto);
    Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateGradeDto dto);
    Task<ReturnResult<EnrollmentDto>> DeleteAsync(int id);
}
