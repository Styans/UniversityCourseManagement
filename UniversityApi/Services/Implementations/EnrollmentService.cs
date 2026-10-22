using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly ICourseRepository _courses;
    private readonly IMapper _mapper;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students,
        ICourseRepository courses, IMapper mapper, ILogger<EnrollmentService> logger)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IReadOnlyList<EnrollmentDto>>> GetAllAsync(EnrollmentFilterDto filter)
    {
        var enrollments = await _enrollments.GetFilteredAsync(filter.StudentId, filter.CourseId);
        return ReturnResult<IReadOnlyList<EnrollmentDto>>.Success(_mapper.Map<List<EnrollmentDto>>(enrollments));
    }

    public async Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id)
    {
        var enrollment = await _enrollments.GetWithDetailsAsync(id);
        if (enrollment is null)
            return EnrollmentNotFound(id);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto)
    {
        var studentId = dto.StudentId!.Value;
        var courseId = dto.CourseId!.Value;

        // Бизнес-правило №2: нельзя записать несуществующего студента
        if (!await _students.ExistsAsync(studentId))
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} does not exist", studentId);
            return ReturnResult<EnrollmentDto>.NotFound(ErrorCodes.StudentNotFound,
                $"Student with id {studentId} not found");
        }

        // Бизнес-правило №1: нельзя записать на несуществующий курс
        if (!await _courses.ExistsAsync(courseId))
        {
            _logger.LogWarning("Enrollment rejected: course {CourseId} does not exist", courseId);
            return ReturnResult<EnrollmentDto>.NotFound(ErrorCodes.CourseNotFound,
                $"Course with id {courseId} not found");
        }

        // Бизнес-правило №3: студент не может быть записан на один курс дважды
        if (await _enrollments.IsStudentEnrolledAsync(studentId, courseId))
        {
            _logger.LogWarning("Enrollment rejected: student {StudentId} is already enrolled in course {CourseId}",
                studentId, courseId);
            return ReturnResult<EnrollmentDto>.Conflict(ErrorCodes.StudentAlreadyEnrolled,
                $"Student {studentId} is already enrolled in course {courseId}");
        }

        var enrollment = _mapper.Map<Enrollment>(dto);
        await _enrollments.AddAsync(enrollment);

        _logger.LogInformation("Enrollment created: Id={EnrollmentId}, StudentId={StudentId}, CourseId={CourseId}",
            enrollment.Id, studentId, courseId);

        var created = await _enrollments.GetWithDetailsAsync(enrollment.Id);
        return ReturnResult<EnrollmentDto>.Created(_mapper.Map<EnrollmentDto>(created));
    }

    public async Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentUpdateGradeDto dto)
    {
        var enrollment = await _enrollments.GetByIdAsync(id);
        if (enrollment is null)
            return EnrollmentNotFound(id);

        var oldGrade = enrollment.Grade;
        enrollment.Grade = dto.Grade;
        await _enrollments.UpdateAsync(enrollment);

        _logger.LogInformation("Enrollment grade updated: Id={EnrollmentId}, StudentId={StudentId}, CourseId={CourseId}, Grade {OldGrade} -> {NewGrade}",
            enrollment.Id, enrollment.StudentId, enrollment.CourseId, oldGrade?.ToString() ?? "none", enrollment.Grade);

        var updated = await _enrollments.GetWithDetailsAsync(id);
        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(updated));
    }

    public async Task<ReturnResult<EnrollmentDto>> DeleteAsync(int id)
    {
        var details = await _enrollments.GetWithDetailsAsync(id);
        if (details is null)
            return EnrollmentNotFound(id);

        var tracked = await _enrollments.GetByIdAsync(id);
        await _enrollments.DeleteAsync(tracked!);

        _logger.LogInformation("Enrollment deleted: Id={EnrollmentId}, student {StudentId} removed from course {CourseId}",
            id, details.StudentId, details.CourseId);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(details));
    }

    private ReturnResult<EnrollmentDto> EnrollmentNotFound(int id)
    {
        _logger.LogWarning("Enrollment not found: Id={EnrollmentId}", id);
        return ReturnResult<EnrollmentDto>.NotFound(ErrorCodes.EnrollmentNotFound, $"Enrollment with id {id} not found");
    }
}
