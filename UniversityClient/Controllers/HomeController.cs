using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UniversityClient.Services;

namespace UniversityClient.Controllers;

public class HomeController : Controller
{
    private readonly UniversityApiOptions _options;

    public HomeController(IOptions<UniversityApiOptions> options)
    {
        _options = options.Value;
    }

    public IActionResult Index() => RedirectToAction("Index", "Courses");

    /// <summary>
    /// Демонстрация CORS: запрос к API выполняет сам браузер (fetch), а не сервер клиента.
    /// Браузер разрешит прочитать ответ, только если origin этой страницы есть в Cors:AllowedOrigins API.
    /// </summary>
    public IActionResult Cors()
    {
        ViewData["ApiBaseUrl"] = _options.BaseUri.ToString();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
