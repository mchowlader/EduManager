using EduSystem.ApplicationUsers.Api.DependencyResolver;
using EduSystem.ApplicationUsers.Api.EndPoints;
using EduSystem.ApplicationUsers.Application.DependencyResolver;
using EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;
using Serilog;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMultiTenancy();
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog(logger);

builder.Services
    .AddSwaggerConfiguration(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseExceptionHandler("/api/error");
app.UseHttpsRedirection();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");

    options.DefaultModelsExpandDepth(-1); // Models section collapse
    options.DisplayRequestDuration(); // Request duration 
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});
app.UseAuthentication();
app.UseMultiTenancy();
app.UseAuthorization();
app.MapEndpoints();
app.Run();
