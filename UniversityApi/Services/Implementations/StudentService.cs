using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using UniversityApi.Auth;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

/// <summary>
/// Бизнес-логика по студентам: проверки, маппинг, логирование.
/// Контроллер только передаёт сюда запрос и возвращает результат.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IStudentRepository _students;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorization;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository students, IMapper mapper, IAuthorizationService authorization,
        ICurrentUser currentUser, ILogger<StudentService> logger)
    {
        _students = students;
        _mapper = mapper;
        _authorization = authorization;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReturnResult<IReadOnlyList<StudentDto>>> GetAllAsync(string? search)
    {
        var students = await _students.GetAllAsync(search);
        return ReturnResult<IReadOnlyList<StudentDto>>.Success(_mapper.Map<List<StudentDto>>(students));
    }

    public async Task<ReturnResult<StudentDto>> GetByIdAsync(int id)
    {
        // Проверка принадлежности: Student видит только свой профиль.
        // Выполняется до поиска, чтобы по 404/403 нельзя было перебором узнать, какие id существуют.
        if (!await CanReadAsync(id))
            return AccessDenied<StudentDto>(id);

        var student = await _students.GetByIdAsync(id);
        if (student is null)
            return StudentNotFound<StudentDto>(id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDetailsDto>> GetWithCoursesAsync(int id)
    {
        // Оценки студента — персональные данные: Student не может смотреть чужие
        if (!await CanReadAsync(id))
            return AccessDenied<StudentDetailsDto>(id);

        var student = await _students.GetWithCoursesAsync(id);
        if (student is null)
            return StudentNotFound<StudentDetailsDto>(id);

        return ReturnResult<StudentDetailsDto>.Success(_mapper.Map<StudentDetailsDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto)
    {
        if (await _students.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Student was not created: email {Email} is already used", dto.Email);
            return ReturnResult<StudentDto>.Conflict(ErrorCodes.StudentEmailAlreadyExists,
                $"Student with email '{dto.Email.Trim().ToLowerInvariant()}' already exists");
        }

        var student = _mapper.Map<Student>(dto);
        await _students.AddAsync(student);

        _logger.LogInformation("Student created: Id={StudentId}, Name={FirstName} {LastName}, Email={Email}",
            student.Id, student.FirstName, student.LastName, student.Email);

        return ReturnResult<StudentDto>.Created(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
            return StudentNotFound<StudentDto>(id);

        if (await _students.EmailExistsAsync(dto.Email, excludeId: id))
        {
            _logger.LogWarning("Student {StudentId} was not updated: email {Email} is already used", id, dto.Email);
            return ReturnResult<StudentDto>.Conflict(ErrorCodes.StudentEmailAlreadyExists,
                $"Student with email '{dto.Email.Trim().ToLowerInvariant()}' already exists");
        }

        _mapper.Map(dto, student); // копирует значения DTO в отслеживаемую сущность
        await _students.UpdateAsync(student);

        _logger.LogInformation("Student updated: Id={StudentId}, Name={FirstName} {LastName}, Email={Email}",
            student.Id, student.FirstName, student.LastName, student.Email);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> DeleteAsync(int id)
    {
        var student = await _students.GetByIdAsync(id);
        if (student is null)
            return StudentNotFound<StudentDto>(id);

        await _students.DeleteAsync(student);

        _logger.LogInformation("Student deleted: Id={StudentId}, Email={Email} (enrollments removed by cascade)",
            student.Id, student.Email);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    private async Task<bool> CanReadAsync(int studentId)
    {
        var result = await _authorization.AuthorizeAsync(
            _currentUser.Principal, new StudentResource(studentId), ResourceOperations.Read);
        return result.Succeeded;
    }

    private ReturnResult<T> AccessDenied<T>(int studentId)
    {
        _logger.LogWarning("Access denied: user {UserName} tried to read student {StudentId}",
            _currentUser.UserName, studentId);
        return ReturnResult<T>.Forbidden(ErrorCodes.Forbidden, "You can view only your own student profile and grades");
    }

    private ReturnResult<T> StudentNotFound<T>(int id)
    {
        _logger.LogWarning("Student not found: Id={StudentId}", id);
        return ReturnResult<T>.NotFound(ErrorCodes.StudentNotFound, $"Student with id {id} not found");
    }
}
