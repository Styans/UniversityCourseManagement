namespace UniversityApi.DTO.Students;

/// <summary>Студент вместе со списком курсов, на которые он записан.</summary>
public class StudentDetailsDto : StudentDto
{
    public int TotalCredits { get; set; }
    public List<StudentCourseDto> Courses { get; set; } = new();
}

/// <summary>Курс в составе карточки студента.</summary>
public class StudentCourseDto
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int Credits { get; set; }
    public string TeacherFullName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public decimal? Grade { get; set; }
}
