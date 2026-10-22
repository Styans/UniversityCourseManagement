namespace UniversityApi.Common;

/// <summary>Все коды ошибок API в одном месте.</summary>
public static class ErrorCodes
{
    // 400
    public const string ValidationError = "VALIDATION_ERROR";

    // 404
    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string EnrollmentNotFound = "ENROLLMENT_NOT_FOUND";
    public const string EndpointNotFound = "ENDPOINT_NOT_FOUND";

    // 405 / 415
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";

    // 409
    public const string StudentEmailAlreadyExists = "STUDENT_EMAIL_ALREADY_EXISTS";
    public const string TeacherEmailAlreadyExists = "TEACHER_EMAIL_ALREADY_EXISTS";
    public const string StudentAlreadyEnrolled = "STUDENT_ALREADY_ENROLLED";
    public const string TeacherHasCourses = "TEACHER_HAS_COURSES";
    public const string DatabaseConflict = "DATABASE_CONFLICT";

    // 500
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}
