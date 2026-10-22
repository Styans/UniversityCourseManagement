using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UniversityClient.Models;

namespace UniversityClient.Services;

public interface IUniversityApiClient
{
    Task<ApiResult<List<CourseV1Dto>>> GetCoursesV1Async(string? search, CancellationToken cancellationToken = default);
    Task<ApiResult<List<CourseV2Dto>>> GetCoursesV2Async(string? search, CancellationToken cancellationToken = default);
    Task<ApiResult<CourseV1Dto>> GetCourseV1Async(int id, CancellationToken cancellationToken = default);
    Task<ApiResult<CourseV2Dto>> GetCourseV2Async(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP-клиент University Course Management API.
/// HttpClient создаётся через IHttpClientFactory (именованный клиент "UniversityApi"):
/// фабрика переиспользует обработчики соединений и не допускает исчерпания сокетов,
/// которое бывает при создании new HttpClient() на каждый запрос.
/// Клиент получает данные только по HTTP и ничего не знает о базе данных API.
/// </summary>
public class UniversityApiClient : IUniversityApiClient
{
    public const string HttpClientName = "UniversityApi";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UniversityApiClient> _logger;

    public UniversityApiClient(IHttpClientFactory httpClientFactory, ILogger<UniversityApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<ApiResult<List<CourseV1Dto>>> GetCoursesV1Async(string? search, CancellationToken cancellationToken = default)
        => GetAsync<List<CourseV1Dto>>($"api/v1/courses{SearchQuery(search)}", cancellationToken);

    public Task<ApiResult<List<CourseV2Dto>>> GetCoursesV2Async(string? search, CancellationToken cancellationToken = default)
        => GetAsync<List<CourseV2Dto>>($"api/v2/courses{SearchQuery(search)}", cancellationToken);

    public Task<ApiResult<CourseV1Dto>> GetCourseV1Async(int id, CancellationToken cancellationToken = default)
        => GetAsync<CourseV1Dto>($"api/v1/courses/{id}", cancellationToken);

    public Task<ApiResult<CourseV2Dto>> GetCourseV2Async(int id, CancellationToken cancellationToken = default)
        => GetAsync<CourseV2Dto>($"api/v2/courses/{id}", cancellationToken);

    private static string SearchQuery(string? search)
        => string.IsNullOrWhiteSpace(search) ? string.Empty : $"?search={Uri.EscapeDataString(search.Trim())}";

    /// <summary>
    /// GET-запрос с обработкой всех типов сбоев:
    /// API недоступен, тайм-аут, неуспешный HTTP-код (с сообщением из ReturnResult), некорректный ответ.
    /// </summary>
    private async Task<ApiResult<T>> GetAsync<T>(string relativeUrl, CancellationToken cancellationToken) where T : class
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var response = await client.GetAsync(relativeUrl, cancellationToken);
            var body = await TryReadEnvelopeAsync<T>(response, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                if (body?.Result is null)
                {
                    _logger.LogWarning("API returned {Status} for {Url}, but the body could not be parsed",
                        (int)response.StatusCode, relativeUrl);
                    return ApiResult<T>.Fail(ApiErrorKind.InvalidResponse,
                        "API вернул ответ в неожиданном формате.", (int)response.StatusCode);
                }

                return ApiResult<T>.Ok(body.Result);
            }

            var kind = response.StatusCode == HttpStatusCode.NotFound ? ApiErrorKind.NotFound : ApiErrorKind.HttpError;
            var message = body?.ErrorMessage ?? $"API вернул {(int)response.StatusCode} {response.ReasonPhrase}.";

            _logger.LogWarning("API error {Status} {ErrorCode} for {Url}: {Message}",
                (int)response.StatusCode, body?.ErrorCode, relativeUrl, message);

            return ApiResult<T>.Fail(kind, message, (int)response.StatusCode, body?.ErrorCode, body?.TraceId);
        }
        catch (HttpRequestException ex)
        {
            // Соединение не установлено: API не запущен, неверный адрес или порт, нет сети
            _logger.LogWarning("API is unavailable at {BaseAddress}: {Error}", client.BaseAddress, ex.Message);
            return ApiResult<T>.Fail(ApiErrorKind.Unavailable,
                $"Не удалось подключиться к API по адресу {client.BaseAddress}. Проверьте, что University Course Management API запущен.");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient сообщает о тайм-ауте через TaskCanceledException
            _logger.LogWarning("API request timed out after {Timeout}s: {Url}", client.Timeout.TotalSeconds, relativeUrl);
            return ApiResult<T>.Fail(ApiErrorKind.Timeout,
                $"API не ответил за {client.Timeout.TotalSeconds:0} с. Попробуйте позже.");
        }
    }

    private async Task<ApiEnvelope<T>?> TryReadEnvelopeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) where T : class
    {
        if (response.Content.Headers.ContentType?.MediaType != "application/json")
            return null;

        try
        {
            return await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("Failed to parse API response: {Error}", ex.Message);
            return null;
        }
    }
}
