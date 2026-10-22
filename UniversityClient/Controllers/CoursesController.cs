using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniversityClient.Models;
using UniversityClient.Services;

namespace UniversityClient.Controllers;

/// <summary>Список курсов и карточка курса — данные запрашиваются у API по HTTP.</summary>
public class CoursesController : Controller
{
    private readonly IUniversityApiClient _api;
    private readonly UniversityApiOptions _options;

    public CoursesController(IUniversityApiClient api, IOptions<UniversityApiOptions> options)
    {
        _api = api;
        _options = options.Value;
    }

    // GET /courses?search=data&version=v1
    public async Task<IActionResult> Index(string? search, string? version, CancellationToken cancellationToken)
    {
        var model = new CourseListViewModel { Version = NormalizeVersion(version), Search = search };

        if (model.Version == "v1")
        {
            var result = await _api.GetCoursesV1Async(search, cancellationToken);
            if (result.IsSuccess)
                model.Courses = result.Data!.Select(CourseViewModel.From).ToList();
            else
                model.Error = ApiErrorViewModel.From(result);
        }
        else
        {
            var result = await _api.GetCoursesV2Async(search, cancellationToken);
            if (result.IsSuccess)
                model.Courses = result.Data!.Select(CourseViewModel.From).ToList();
            else
                model.Error = ApiErrorViewModel.From(result);
        }

        if (model.Error?.Kind is ApiErrorKind.Unavailable or ApiErrorKind.Timeout)
            Response.StatusCode = StatusCodes.Status502BadGateway;

        return View(model);
    }

    // GET /courses/details/5?version=v1
    public async Task<IActionResult> Details(int id, string? version, CancellationToken cancellationToken)
    {
        var model = new CourseDetailsViewModel { Id = id, Version = NormalizeVersion(version) };

        if (model.Version == "v1")
        {
            var result = await _api.GetCourseV1Async(id, cancellationToken);
            model.Course = result.IsSuccess ? CourseViewModel.From(result.Data!) : null;
            model.Error = result.IsSuccess ? null : ApiErrorViewModel.From(result);
        }
        else
        {
            var result = await _api.GetCourseV2Async(id, cancellationToken);
            model.Course = result.IsSuccess ? CourseViewModel.From(result.Data!) : null;
            model.Error = result.IsSuccess ? null : ApiErrorViewModel.From(result);
        }

        Response.StatusCode = model.Error?.Kind switch
        {
            ApiErrorKind.NotFound => StatusCodes.Status404NotFound,
            ApiErrorKind.Unavailable or ApiErrorKind.Timeout => StatusCodes.Status502BadGateway,
            null => StatusCodes.Status200OK,
            _ => StatusCodes.Status502BadGateway
        };

        return View(model);
    }

    private string NormalizeVersion(string? version)
        => version is "v1" or "v2" ? version : _options.DefaultVersion;
}
