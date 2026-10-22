using System.ComponentModel.DataAnnotations;

namespace UniversityClient.Services;

/// <summary>
/// Настройки подключения к University Course Management API (секция "UniversityApi" в appsettings.json).
/// Адрес API не захардкожен в коде — его можно поменять в конфигурации или переменной окружения
/// UniversityApi__BaseUrl.
/// </summary>
public class UniversityApiOptions
{
    public const string SectionName = "UniversityApi";

    /// <summary>Базовый адрес API, например http://localhost:5096/.</summary>
    [Required(ErrorMessage = "UniversityApi:BaseUrl is required in appsettings.json")]
    [Url(ErrorMessage = "UniversityApi:BaseUrl must be an absolute URL")]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Тайм-аут HTTP-запроса в секундах.</summary>
    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>Версия API по умолчанию: v1 или v2.</summary>
    [RegularExpression("^v[12]$")]
    public string DefaultVersion { get; set; } = "v2";

    public Uri BaseUri => new(BaseUrl.EndsWith('/') ? BaseUrl : BaseUrl + "/");
}
