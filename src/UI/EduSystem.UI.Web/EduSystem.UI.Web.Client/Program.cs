//using EduSystem.UI.Web.Client.Services;
//using EduSystem.UI.Web.Client.Services.Auth;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using MudBlazor.Services;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);


//builder.Services.AddMudServices();
//builder.Services.AddScoped<IStudentService, MockStudentService>();
//builder.Services.AddScoped<ITeacherService, MockTeacherService>();
//builder.Services.AddScoped(sp =>
//    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
//);
//builder.Services.AddAuthorizationCore();
//builder.Services.AddScoped<CustomAuthenticationStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
//    provider.GetRequiredService<CustomAuthenticationStateProvider>());

//await builder.Build().RunAsync();

using EduSystem.UI.Web.Client.Services;
using EduSystem.UI.Web.Client.Services.Auth;
using EduSystem.UI.Web.Client.Services.Base;
using EduSystem.UI.Web.Client.HttpHandlers;
using EduSystem.UI.Web.Client.Services.Academy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();
builder.Services.AddScoped<IStudentService, MockStudentService>();
builder.Services.AddScoped<ITeacherService, MockTeacherService>();
builder.Services.AddScoped<ITenantService, TenantService>();

// ✅ Industrial Standard API Services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAcademyService, AcademyService>();

// ✅ Gateway URL - appsettings.json থেকে পড়বে
// WASM mode এ appsettings.json wwwroot folder এ থাকে
var gatewayUrl = builder.Configuration["ApiGatewayUrl:GatewayApi"] ?? "https://localhost:44308/";

Console.WriteLine($"[CLIENT CONFIG] Gateway URL: {gatewayUrl}");
Console.WriteLine($"[CLIENT CONFIG] Host Environment Base: {builder.HostEnvironment.BaseAddress}");

// Register Authentication Handler
builder.Services.AddScoped<AuthenticationHandler>();

// ✅ Configure Gateway HttpClient with Authentication
builder.Services.AddHttpClient("GatewayClient", client =>
{
    client.BaseAddress = new Uri(gatewayUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddHttpMessageHandler<AuthenticationHandler>();

// ✅ Default HttpClient (backup) - points to Gateway
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    return httpClient;
});

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthManager>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();
