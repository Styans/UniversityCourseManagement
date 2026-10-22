namespace UniversityApi.Common;

/// <summary>Все коды ошибок API в одном месте.</summary>
public static class ErrorCodes
{
    // 400
    public const string ValidationError = "VALIDATION_ERROR";

    // 401 — пользователь не аутентифицирован
    public const string Unauthorized = "UNAUTHORIZED";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string AccountLocked = "ACCOUNT_LOCKED";

    // 403 — аутентифицирован, но нет прав
    public const string Forbidden = "FORBIDDEN";
    public const string NotCourseTeacher = "NOT_COURSE_TEACHER";
    public const string AccountNotLinked = "ACCOUNT_NOT_LINKED";

    // 404
    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string EnrollmentNotFound = "ENROLLMENT_NOT_FOUND";
    public const string EndpointNotFound = "ENDPOINT_NOT_FOUND";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string RoleNotFound = "ROLE_NOT_FOUND";

    // 400 (версии API)
    public const string UnsupportedApiVersion = "UNSUPPORTED_API_VERSION";

    // 405 / 415
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";

    // 409
    public const string StudentEmailAlreadyExists = "STUDENT_EMAIL_ALREADY_EXISTS";
    public const string TeacherEmailAlreadyExists = "TEACHER_EMAIL_ALREADY_EXISTS";
    public const string StudentAlreadyEnrolled = "STUDENT_ALREADY_ENROLLED";
    public const string TeacherHasCourses = "TEACHER_HAS_COURSES";
    public const string DatabaseConflict = "DATABASE_CONFLICT";
    public const string UserNameAlreadyExists = "USERNAME_ALREADY_EXISTS";
    public const string UserEmailAlreadyExists = "USER_EMAIL_ALREADY_EXISTS";
    public const string RoleAlreadyExists = "ROLE_ALREADY_EXISTS";
    public const string ProfileAlreadyLinked = "PROFILE_ALREADY_LINKED";
    public const string CannotRemoveOwnAdminRole = "CANNOT_REMOVE_OWN_ADMIN_ROLE";

    // 500
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
}
