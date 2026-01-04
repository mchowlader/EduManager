using System.Security.Claims;
using EduSystem.Shared.Infrastructure.Extensions;
using EduSystem.UI.Web.Client.Services;
using EduSystem.UI.Web.Client.Services.Academy;
using EduSystem.UI.Web.Client.Services.Auth;
using EduSystem.UI.Web.Client.Services.Base;
using EduSystem.UI.Web.Components;
using EduSystem.UI.Web.Services;
using EduSystem.UI.Web.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCentralizedLoggin("WEBEDU");

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

// Existing services
builder.Services.AddScoped<IStudentService, MockStudentService>();
builder.Services.AddScoped<ITeacherService, MockTeacherService>();

// Tenant Service Registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EduSystem.UI.Web.Services.ITenantService, EduSystem.UI.Web.Services.TenantService>();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAcademyService, AcademyService>();
builder.Services.AddScoped<IClassesService, ClassesService>();

// Cookie Authentication for Server-side
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EduSystem.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.LoginPath = "/access-denied";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Register CustomAuthenticationStateProvider
builder.Services.AddScoped<CustomServerAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomServerAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthManager>(sp => sp.GetRequiredService<CustomServerAuthenticationStateProvider>());
builder.Services.AddScoped<CustomAuthenticationStateProvider>(); // Keep for WASM sync if needed

// ✅ All API calls will go through the Gateway URL
var gatewayUrl = builder.Configuration["ApiGatewayUrl:GatewayApi"] ?? "https://localhost:44308/";
Console.WriteLine($"[SERVER CONFIG] Gateway URL: {gatewayUrl}");

// Register Authentication Handler for Server side (industrial grade)
builder.Services.AddScoped<EduSystem.UI.Web.Client.HttpHandlers.AuthenticationHandler>();

// ✅ Main Gateway Client (Server side rendering )
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<EduSystem.UI.Web.Client.HttpHandlers.AuthenticationHandler>();

// Backward compatibility
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ❌ These separate service clients are not needed
// All calls should go through GatewayClient
// However, if you still want to keep them for compatibility:

builder.Services.AddHttpClient("IdentityApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/identity/");
});
builder.Services.AddHttpClient("ApplicationUserApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/ApplicationUser/");
});
builder.Services.AddHttpClient("AdminApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/admin/");
});

builder.Services.AddHttpClient("AttendanceApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/attendance/");
});

builder.Services.AddHttpClient("BillingApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/billing/");
});

builder.Services.AddHttpClient("NotificationApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/notifications/");
});

builder.Services.AddHttpClient("ReportApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/report/");
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.UseAuthentication();

// Auth Sync Middleware - JWT Cookie থেকে Claims populate করে
// Run AFTER UseAuthentication to ensure we can provide a fallback if the default cookie auth fails
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        if (context.Request.Cookies.TryGetValue("edu_auth_token", out var token) && !string.IsNullOrEmpty(token))
        {
            try
            {
                var claims = JwtClaimParser.ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                context.User = new ClaimsPrincipal(identity);

                Console.WriteLine($"[SERVER AUTH] User '{context.User.Identity?.Name}' authenticated via fallback cookie.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER AUTH] Invalid token in cookie: {ex.Message}");
            }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EduSystem.UI.Web.Client._Imports).Assembly);

app.Run();
