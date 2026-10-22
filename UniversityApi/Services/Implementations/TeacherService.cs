using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ITeacherRepository teachers, IMapper mapper, ILogger<TeacherService> logger)
    {
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IReadOnlyList<TeacherDto>>> GetAllAsync(string? department)
    {
        var teachers = await _teachers.GetAllAsync(department);
        return ReturnResult<IReadOnlyList<TeacherDto>>.Success(_mapper.Map<List<TeacherDto>>(teachers));
    }

    public async Task<ReturnResult<TeacherDto>> GetByIdAsync(int id)
    {
        var teacher = await _teachers.GetByIdAsync(id);
        if (teacher is null)
            return TeacherNotFound(id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto)
    {
        if (await _teachers.EmailExistsAsync(dto.Email))
        {
            _logger.LogWarning("Teacher was not created: email {Email} is already used", dto.Email);
            return ReturnResult<TeacherDto>.Conflict(ErrorCodes.TeacherEmailAlreadyExists,
                $"Teacher with email '{dto.Email.Trim().ToLowerInvariant()}' already exists");
        }

        var teacher = _mapper.Map<Teacher>(dto);
        await _teachers.AddAsync(teacher);

        _logger.LogInformation("Teacher created: Id={TeacherId}, Name={FirstName} {LastName}, Department={Department}",
            teacher.Id, teacher.FirstName, teacher.LastName, teacher.Department);

        return ReturnResult<TeacherDto>.Created(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto)
    {
        var teacher = await _teachers.GetByIdAsync(id);
        if (teacher is null)
            return TeacherNotFound(id);

        if (await _teachers.EmailExistsAsync(dto.Email, excludeId: id))
        {
            _logger.LogWarning("Teacher {TeacherId} was not updated: email {Email} is already used", id, dto.Email);
            return ReturnResult<TeacherDto>.Conflict(ErrorCodes.TeacherEmailAlreadyExists,
                $"Teacher with email '{dto.Email.Trim().ToLowerInvariant()}' already exists");
        }

        _mapper.Map(dto, teacher);
        await _teachers.UpdateAsync(teacher);

        _logger.LogInformation("Teacher updated: Id={TeacherId}, Name={FirstName} {LastName}, Department={Department}",
            teacher.Id, teacher.FirstName, teacher.LastName, teacher.Department);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> DeleteAsync(int id)
    {
        var teacher = await _teachers.GetByIdAsync(id);
        if (teacher is null)
            return TeacherNotFound(id);

        if (await _teachers.HasCoursesAsync(id))
        {
            _logger.LogWarning("Teacher {TeacherId} was not deleted: there are courses assigned to this teacher", id);
            return ReturnResult<TeacherDto>.Conflict(ErrorCodes.TeacherHasCourses,
                $"Teacher with id {id} has assigned courses. Reassign or delete the courses first");
        }

        await _teachers.DeleteAsync(teacher);

        _logger.LogInformation("Teacher deleted: Id={TeacherId}, Email={Email}", teacher.Id, teacher.Email);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    private ReturnResult<TeacherDto> TeacherNotFound(int id)
    {
        _logger.LogWarning("Teacher not found: Id={TeacherId}", id);
        return ReturnResult<TeacherDto>.NotFound(ErrorCodes.TeacherNotFound, $"Teacher with id {id} not found");
    }
}
