using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;
using UniversityClient.Services;

var builder = WebApplication.CreateBuilder(args);

// Адрес API — из appsettings.json (секция UniversityApi), с проверкой при запуске
builder.Services.AddOptions<UniversityApiOptions>()
    .Bind(builder.Configuration.GetSection(UniversityApiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// IHttpClientFactory: именованный HttpClient с базовым адресом, тайм-аутом и заголовком Accept
builder.Services.AddHttpClient(UniversityApiClient.HttpClientName, (services, client) =>
{
    var options = services.GetRequiredService<IOptions<UniversityApiOptions>>().Value;
    client.BaseAddress = options.BaseUri;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddScoped<IUniversityApiClient, UniversityApiClient>();

builder.Services.AddControllersWithViews();

// Кириллица в HTML без преобразования в &#x...; (по умолчанию Razor кодирует всё, кроме латиницы)
builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Courses}/{action=Index}/{id?}");

app.Run();
