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
    .AddApiServices(builder.Configuration)  // ⚠️ API versioning আগে configure করুন
    .AddSwaggerConfiguration()              // তারপর Swagger
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    // ✅ v2 প্রথমে (default)
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "v2 - Latest");
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 - Deprecated");

    options.DefaultModelsExpandDepth(-1); // Models section collapse
    options.DisplayRequestDuration(); // Request duration দেখান
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // সব collapse থাকবে
});

app.UseHttpsRedirection();
app.UseCors();
app.UseExceptionHandler("/api/error");
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();
