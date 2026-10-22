using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UniversityApi.Auth;
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
    private readonly IAuthorizationService _authorization;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IEnrollmentRepository enrollments, IStudentRepository students,
        ICourseRepository courses, IMapper mapper, IAuthorizationService authorization,
        ICurrentUser currentUser, ILogger<EnrollmentService> logger)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
        _mapper = mapper;
        _authorization = authorization;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Список записей с учётом роли:
    /// Admin — все; Teacher — только по своим курсам; Student — только свои.
    /// Попытка явно запросить чужие данные (studentId / courseId) — 403, а не пустой список.
    /// </summary>
    public async Task<ReturnResult<IReadOnlyList<EnrollmentDto>>> GetAllAsync(EnrollmentFilterDto filter)
    {
        int? studentId = filter.StudentId;
        int? teacherId = null;

        if (!_currentUser.IsInRole(AppRoles.Admin))
        {
            if (_currentUser.IsInRole(AppRoles.Teacher))
            {
                if (_currentUser.TeacherId is not { } ownTeacherId)
                    return NotLinked<IReadOnlyList<EnrollmentDto>>("Teacher");

                if (filter.CourseId is { } courseId)
                {
                    var course = await _courses.GetByIdAsync(courseId);
                    if (course is not null && course.TeacherId != ownTeacherId)
                        return Denied<IReadOnlyList<EnrollmentDto>>(
                            $"read enrollments of course {courseId}", ErrorCodes.NotCourseTeacher,
                            "You can view enrollments only for your own courses");
                }

                teacherId = ownTeacherId;
            }
            else if (_currentUser.IsInRole(AppRoles.Student))
            {
                if (_currentUser.StudentId is not { } ownStudentId)
                    return NotLinked<IReadOnlyList<EnrollmentDto>>("Student");

                if (filter.StudentId.HasValue && filter.StudentId != ownStudentId)
                    return Denied<IReadOnlyList<EnrollmentDto>>(
                        $"read grades of student {filter.StudentId}", ErrorCodes.Forbidden,
                        "You can view only your own enrollments and grades");

                studentId = ownStudentId;
            }
            else
            {
                return Denied<IReadOnlyList<EnrollmentDto>>("read enrollments", ErrorCodes.Forbidden,
                    "Your account has no role with access to enrollments");
            }
        }

        var enrollments = await _enrollments.GetFilteredAsync(studentId, filter.CourseId, teacherId);
        return ReturnResult<IReadOnlyList<EnrollmentDto>>.Success(_mapper.Map<List<EnrollmentDto>>(enrollments));
    }

    public async Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id)
    {
        var enrollment = await _enrollments.GetWithDetailsAsync(id);
        if (enrollment is null)
            return EnrollmentNotFound(id);

        if (!await IsAuthorizedAsync(enrollment, ResourceOperations.Read))
            return Denied<EnrollmentDto>($"read enrollment {id}", ErrorCodes.Forbidden,
                "You can view only your own enrollments or enrollments of your courses");

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
        // Сначала читаем запись вместе с курсом: нужен TeacherId курса для проверки принадлежности
        var details = await _enrollments.GetWithDetailsAsync(id);
        if (details is null)
            return EnrollmentNotFound(id);

        // Роль Teacher уже проверена политикой GradeManagement, но этого недостаточно:
        // преподаватель может менять оценки только по СВОИМ курсам
        if (!await IsAuthorizedAsync(details, ResourceOperations.UpdateGrade))
            return Denied<EnrollmentDto>($"change grade in enrollment {id} (course {details.CourseId})",
                ErrorCodes.NotCourseTeacher, "You can change grades only for your own courses");

        var enrollment = (await _enrollments.GetByIdAsync(id))!;

        var oldGrade = enrollment.Grade;
        enrollment.Grade = dto.Grade;
        await _enrollments.UpdateAsync(enrollment);

        _logger.LogInformation("Enrollment grade updated: Id={EnrollmentId}, StudentId={StudentId}, CourseId={CourseId}, Grade {OldGrade} -> {NewGrade}, by {UserName}",
            enrollment.Id, enrollment.StudentId, enrollment.CourseId, oldGrade?.ToString() ?? "none", enrollment.Grade,
            _currentUser.UserName);

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

    private async Task<bool> IsAuthorizedAsync(Enrollment enrollment, OperationAuthorizationRequirement operation)
    {
        var resource = new EnrollmentResource(enrollment.StudentId, enrollment.Course.TeacherId);
        var result = await _authorization.AuthorizeAsync(_currentUser.Principal, resource, operation);
        return result.Succeeded;
    }

    private ReturnResult<T> Denied<T>(string action, string errorCode, string message)
    {
        _logger.LogWarning("Access denied: user {UserName} tried to {Action}", _currentUser.UserName, action);
        return ReturnResult<T>.Forbidden(errorCode, message);
    }

    private ReturnResult<T> NotLinked<T>(string profile)
    {
        _logger.LogWarning("Access denied: user {UserName} is not linked to a {Profile} profile", _currentUser.UserName, profile);
        return ReturnResult<T>.Forbidden(ErrorCodes.AccountNotLinked,
            $"Your account is not linked to a {profile} profile. Ask an administrator to link it");
    }

    private ReturnResult<EnrollmentDto> EnrollmentNotFound(int id)
    {
        _logger.LogWarning("Enrollment not found: Id={EnrollmentId}", id);
        return ReturnResult<EnrollmentDto>.NotFound(ErrorCodes.EnrollmentNotFound, $"Enrollment with id {id} not found");
    }
}
