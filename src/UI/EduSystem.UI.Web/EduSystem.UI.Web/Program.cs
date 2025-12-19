//using EduSystem.UI.Web.Client.Pages;
//using EduSystem.UI.Web.Client.Services;
//using EduSystem.UI.Web.Components;
//using EduSystem.UI.Web.Services;
//using MudBlazor.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents()
//    .AddInteractiveWebAssemblyComponents();

//builder.Services.AddMudServices();
//builder.Services.AddScoped<IStudentService, MockStudentService>();
//builder.Services.AddScoped<ITeacherService, MockTeacherService>();

//// Tenant Service Registration
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ITenantService, TenantService>();

//builder.Services.AddMemoryCache();
//builder.Services.AddScoped<AuthService>();

//// HttpClient for API calls
//builder.Services.AddHttpClient<AuthService>(client =>
//{
//    client.BaseAddress = new Uri("https://your-identity-api.com");
//});

//// HttpClient for API calls
//var identityUrl = builder.Configuration["ServiceUrls:IdentityApi"];
//builder.Services.AddHttpClient("IdentityApi", client =>
//{
//    var identityUrl = builder.Configuration["ServiceUrls:IdentityApi"];
//    client.BaseAddress = new Uri(identityUrl ?? "https://localhost:7242/");
//});

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
builder.Services.AddCascadingAuthenticationState();  // <-- এটা MUST থাকতে হবে

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EduSystem.UI.Web.Client._Imports).Assembly);

app.Run();
