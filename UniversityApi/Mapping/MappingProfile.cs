using AutoMapper;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Courses.V2;
using UniversityApi.DTO.Enrollments;
using UniversityApi.DTO.Students;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;

namespace UniversityApi.Mapping;

/// <summary>
/// Правила преобразования Entity ↔ DTO.
/// Поля с одинаковыми именами AutoMapper сопоставляет сам;
/// здесь описаны только отличия (вычисляемые поля, нормализация, игнорируемые свойства).
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---------- Students ----------
        CreateMap<Student, StudentDto>();

        CreateMap<Student, StudentDetailsDto>()
            .ForMember(d => d.Courses, o => o.MapFrom(s => s.Enrollments))
            .ForMember(d => d.TotalCredits, o => o.MapFrom(s => s.Enrollments.Sum(e => e.Course.Credits)));

        CreateMap<Enrollment, StudentCourseDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Course.Credits))
            .ForMember(d => d.TeacherFullName,
                o => o.MapFrom(s => s.Course.Teacher.FirstName + " " + s.Course.Teacher.LastName));

        CreateMap<StudentCreateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate!.Value));

        CreateMap<StudentUpdateDto, Student>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.BirthDate, o => o.MapFrom(s => s.BirthDate!.Value));

        // ---------- Teachers ----------
        CreateMap<Teacher, TeacherDto>();

        CreateMap<TeacherCreateDto, Teacher>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Courses, o => o.Ignore())
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Trim()));

        CreateMap<TeacherUpdateDto, Teacher>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Courses, o => o.Ignore())
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName.Trim()))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Trim().ToLowerInvariant()))
            .ForMember(d => d.Department, o => o.MapFrom(s => s.Department.Trim()));

        // ---------- Courses ----------
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherFullName,
                o => o.MapFrom(s => s.Teacher.FirstName + " " + s.Teacher.LastName));

        // v2: преподаватель — вложенный объект; количество студентов заполняет CourseService
        CreateMap<Course, CourseV2Dto>()
            .ForMember(d => d.EnrolledStudentsCount, o => o.Ignore());

        CreateMap<Teacher, CourseTeacherV2Dto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        CreateMap<CourseCreateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Teacher, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Credits!.Value))
            .ForMember(d => d.TeacherId, o => o.MapFrom(s => s.TeacherId!.Value));

        CreateMap<CourseUpdateDto, Course>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.Teacher, o => o.Ignore())
            .ForMember(d => d.Enrollments, o => o.Ignore())
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name.Trim()))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Credits!.Value))
            .ForMember(d => d.TeacherId, o => o.MapFrom(s => s.TeacherId!.Value));

        // ---------- Enrollments ----------
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentFullName,
                o => o.MapFrom(s => s.Student.FirstName + " " + s.Student.LastName))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name));

        CreateMap<EnrollmentCreateDto, Enrollment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.EnrollmentDate, o => o.Ignore())
            .ForMember(d => d.Grade, o => o.Ignore())
            .ForMember(d => d.Student, o => o.Ignore())
            .ForMember(d => d.Course, o => o.Ignore())
            .ForMember(d => d.StudentId, o => o.MapFrom(s => s.StudentId!.Value))
            .ForMember(d => d.CourseId, o => o.MapFrom(s => s.CourseId!.Value));
    }
}
