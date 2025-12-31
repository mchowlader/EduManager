using EduSystem.Identity.Api.DependencyResolver;
using EduSystem.Identity.Api.Endpoints;
using EduSystem.Identity.Application.DependencyResolver;
using EduSystem.Identity.Infrastructure.DependencyResolver;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(logger);

builder.Services
    .AddApiServices(builder.Configuration) 
    .AddSwaggerConfiguration()              
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");

    options.DefaultModelsExpandDepth(-1); // Models section collapse
    options.DisplayRequestDuration(); // Request duration 
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseHttpsRedirection();
app.UseCors();
app.UseExceptionHandler("/api/error");
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
