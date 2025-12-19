using EduSystem.UI.Web.Client.Services;
using EduSystem.UI.Web.Client.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


builder.Services.AddMudServices();
builder.Services.AddScoped<IStudentService, MockStudentService>();
builder.Services.AddScoped<ITeacherService, MockTeacherService>();
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
);
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

await builder.Build().RunAsync();
