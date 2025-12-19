using EduSystem.UI.Web.Client.Pages;
using EduSystem.UI.Web.Client.Services;
using EduSystem.UI.Web.Client.Services.Auth;
using EduSystem.UI.Web.Components;
using EduSystem.UI.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

// Existing services
builder.Services.AddScoped<IStudentService, MockStudentService>();
builder.Services.AddScoped<ITeacherService, MockTeacherService>();

// Tenant Service Registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuthService>();

// Authentication - Register CustomAuthenticationStateProvider
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EduSystem.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.LoginPath = "/access-denied";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();  

// Register CustomAuthenticationStateProvider for both server and client
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// HttpClient for API calls
var identityUrl = builder.Configuration["ServiceUrls:IdentityApi"];
builder.Services.AddHttpClient("IdentityApi", client =>
{
    client.BaseAddress = new Uri(identityUrl ?? "https://localhost:7242/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// --- Auth Sync Middleware ---
// This middleware bridges the gap between client-side localStorage and server-side SSR.
// It reads the 'edu_auth_token' cookie set by the client and populates the HttpContext.User.
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["edu_auth_token"];
    if (!string.IsNullOrEmpty(token))
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            context.User = new ClaimsPrincipal(identity);
        }
        catch { /* Invalid token, ignore */ }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EduSystem.UI.Web.Client._Imports).Assembly);

app.Run();
