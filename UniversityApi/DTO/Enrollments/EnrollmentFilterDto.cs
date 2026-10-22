using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace UniversityApi.DTO.Enrollments;

/// <summary>Фильтр списка записей: GET /api/enrollments?studentId=1&amp;courseId=2</summary>
public class EnrollmentFilterDto
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive number")]
    [FromQuery(Name = "studentId")]
    public int? StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive number")]
    [FromQuery(Name = "courseId")]
    public int? CourseId { get; set; }
}
