//using EduSystem.UI.Web.Client.Pages;
//using EduSystem.UI.Web.Client.Services;
//using EduSystem.UI.Web.Client.Services.Auth;
//using EduSystem.UI.Web.Components;
//using EduSystem.UI.Web.Services;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using MudBlazor.Services;
//using System.Security.Claims;
//using System.IdentityModel.Tokens.Jwt;
//using Microsoft.AspNetCore.Components.Authorization;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents()
//    .AddInteractiveWebAssemblyComponents();

//builder.Services.AddMudServices();

//// Existing services
//builder.Services.AddScoped<IStudentService, MockStudentService>();
//builder.Services.AddScoped<ITeacherService, MockTeacherService>();

//// Tenant Service Registration
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ITenantService, TenantService>();
//builder.Services.AddMemoryCache();
//builder.Services.AddScoped<AuthService>();

//// Authentication - Register CustomAuthenticationStateProvider
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.Cookie.Name = "EduSystem.Auth";
//        options.Cookie.HttpOnly = true;
//        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//        options.Cookie.SameSite = SameSiteMode.Strict;
//        options.ExpireTimeSpan = TimeSpan.FromHours(12);
//        options.SlidingExpiration = true;
//        options.LoginPath = "/access-denied";
//        options.LogoutPath = "/logout";
//        options.AccessDeniedPath = "/access-denied";
//    });
//builder.Services.AddAuthorizationCore();
//builder.Services.AddCascadingAuthenticationState();  

//// Register CustomAuthenticationStateProvider for both server and client
//builder.Services.AddScoped<CustomAuthenticationStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
//    provider.GetRequiredService<CustomAuthenticationStateProvider>());

//// HttpClient for API calls
////var identityUrl = builder.Configuration["ServiceUrls:IdentityApi"];
////builder.Services.AddHttpClient("IdentityApi", client =>
////{
////    client.BaseAddress = new Uri(identityUrl ?? "https://localhost:7242/");
////});
//var gatewayUrl = builder.Configuration["ApiGatewyUrl:GatewayApi"];
//builder.Services.AddHttpClient("GatewayApi", client =>
//{
//    client.BaseAddress = new Uri(gatewayUrl ?? "https://localhost:7242/");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//});

//// Configure specific service clients if needed
//var identityUrl = builder.Configuration["ServiceUrls:IdentityApi"];
//builder.Services.AddHttpClient("IdentityApi", client =>
//{
//    client.BaseAddress = new Uri(identityUrl ?? "https://localhost:7242/");
//});

//var applicationUserUrl = builder.Configuration["ServiceUrls:ApplicationUserApi"];
//builder.Services.AddHttpClient("ApplicationUserApi", client =>
//{
//    client.BaseAddress = new Uri(applicationUserUrl ?? "https://localhost:7242/");
//});

//var adminUrl = builder.Configuration["ServiceUrls:AdminApi"];
//builder.Services.AddHttpClient("AdminApi", client =>
//{
//    client.BaseAddress = new Uri(adminUrl ?? "https://localhost:7242/");
//});

//var attendanceUrl = builder.Configuration["ServiceUrls:AttendanceApi"];
//builder.Services.AddHttpClient("AttendanceApi", client =>
//{
//    client.BaseAddress = new Uri(attendanceUrl ?? "https://localhost:7242/");
//});

//var billingUrl = builder.Configuration["ServiceUrls:BillingApi"];
//builder.Services.AddHttpClient("BillingApi", client =>
//{
//    client.BaseAddress = new Uri(billingUrl ?? "https://localhost:7242/");
//});

//var notificationsUrl = builder.Configuration["ServiceUrls:NotificationApi"];
//builder.Services.AddHttpClient("NotificationApi", client =>
//{
//    client.BaseAddress = new Uri(billingUrl ?? "https://localhost:7242/");
//});

//var reportUrl = builder.Configuration["ServiceUrls:ReportApi"];
//builder.Services.AddHttpClient("ReportApi", client =>
//{
//    client.BaseAddress = new Uri(reportUrl ?? "https://localhost:7242/");
//});

//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseWebAssemblyDebugging();
//}
//else
//{
//    app.UseExceptionHandler("/Error", createScopeForErrors: true);
//    app.UseHsts();
//}

//app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();
//app.UseAntiforgery();
//app.MapStaticAssets();

//// --- Auth Sync Middleware ---
//// This middleware bridges the gap between client-side localStorage and server-side SSR.
//// It reads the 'edu_auth_token' cookie set by the client and populates the HttpContext.User.
//app.Use(async (context, next) =>
//{
//    var token = context.Request.Cookies["edu_auth_token"];
//    if (!string.IsNullOrEmpty(token))
//    {
//        try
//        {
//            var handler = new JwtSecurityTokenHandler();
//            var jwtToken = handler.ReadJwtToken(token);
//            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
//            context.User = new ClaimsPrincipal(identity);
//        }
//        catch { /* Invalid token, ignore */ }
//    }
//    await next();
//});

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode()
//    .AddInteractiveWebAssemblyRenderMode()
//    .AddAdditionalAssemblies(typeof(EduSystem.UI.Web.Client._Imports).Assembly);

//app.Run();

using EduSystem.UI.Web.Client.Pages;
using EduSystem.UI.Web.Client.Services;
using EduSystem.UI.Web.Client.Services.Auth;
using EduSystem.UI.Web.Components;
using EduSystem.UI.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<AuthService>();

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
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Register CustomAuthenticationStateProvider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// ✅ Gateway URL থেকে সব API call যাবে
var gatewayUrl = builder.Configuration["ApiGatewayUrl:GatewayApi"] ?? "https://localhost:44308/";
Console.WriteLine($"[SERVER CONFIG] Gateway URL: {gatewayUrl}");

// ✅ Main Gateway Client (Server side rendering এর জন্য)
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Backward compatibility
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ❌ এই আলাদা আলাদা service clients এর দরকার নেই
// সব call GatewayClient দিয়ে যাবে
// তবে যদি রাখতেই চান compatibility এর জন্য:
builder.Services.AddHttpClient("IdentityApi", client =>
{
    client.BaseAddress = new Uri($"{gatewayUrl}api/identity/");
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

// Auth Sync Middleware - JWT Cookie থেকে Claims populate করে
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

            Console.WriteLine($"[SERVER AUTH] User authenticated from cookie: {context.User.Identity?.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVER AUTH] Invalid token in cookie: {ex.Message}");
        }
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EduSystem.UI.Web.Client._Imports).Assembly);

Console.WriteLine("[SERVER] Application started successfully");

app.Run();
