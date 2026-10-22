namespace UniversityApi.Common.Validation;

public static class ValidationPatterns
{
    /// <summary>
    /// Более строгая проверка Email, чем встроенный [EmailAddress]:
    /// требуется текст до @, домен и точка в домене (например, name@mail.kz).
    /// </summary>
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$";

    public const string EmailMessage = "Email must be in format name@domain.zone";
}
