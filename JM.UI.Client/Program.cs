using JM.Infrastructure.ExceptionHandler;
using JM.UI.Client.Services;
using JM.UI.Entities.Model;
using JM.UI.Entities.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Options;
using Radzen;
using RadzenBlazorDemos.Server.Data;
using JM.UI.Service;
using JM.UI.DataService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

var builder = WebApplication.CreateBuilder(args);

// Bind configurations
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var appSettings = appSettingsSection.Get<AppSetting>();
builder.Services.AddSingleton(appSettings);
builder.Services.Configure<AppSetting>(appSettingsSection);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents().AddHubOptions(o =>
    {
        o.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    });

// 🔐 Configure Authorization for Blazor Server
builder.Services.AddAuthorizationCore();

// 🔐 Use custom authorization handler that doesn't trigger authentication challenges
// This allows Blazor's AuthorizeRouteView to handle unauthorized access
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, BlazorAuthorizationMiddlewareResultHandler>();

// 🔐 Register Custom Authentication State Provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Add Radzen.Blazor services
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenQueryStringThemeService();
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ExampleService>();
builder.Services.AddDbContextFactory<NorthwindContext>();
builder.Services.AddAIChatService(options =>
    builder.Configuration.GetSection("AIChatService").Bind(options));

builder.Services.AddLocalization();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

builder.Services.AddScoped<CircuitHandlerService>();
builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<CircuitHandlerService>());
builder.Services.AddScoped<ITokenProvider, CircuitTokenProvider>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();


builder.Services.AddHttpClient("MainApi", (serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<AppSetting>>().Value;
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(apiSettings.Timeout);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient("AuthApi", (serviceProvider, client) =>
{
    var apiSettings = serviceProvider.GetRequiredService<IOptions<AppSetting>>().Value;
    client.BaseAddress = new Uri(apiSettings.BaseUrlAuth);
    client.Timeout = TimeSpan.FromSeconds(apiSettings.Timeout);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddSingleton<ILogger, Logger<ErrorDetails>>();
builder.Services.AddDataService();
builder.Services.AddService();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
              "http://144.79.133.21:1003",
              "http://localhost:1003",
              "http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowBlazor");
app.UseAntiforgery();

// 🔐 Authorization middleware (with custom handler to prevent challenges)
app.UseAuthorization();

app.MapRazorPages();
app.MapRazorComponents<JM.UI.Client.App>().AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
public class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // Don't challenge or forbid - just continue to next middleware
        // Blazor's AuthorizeRouteView will handle showing NotAuthorized content
        return next(context);
    }
}