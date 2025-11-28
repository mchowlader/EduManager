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
    .AddSwaggerConfiguration(builder.Configuration)
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler("/api/error");
app.UseHttpsRedirection();
app.UseAuthentication();  
app.UseAuthorization();
app.UseSwaggerConfiguration();
app.MapEndpoints();
app.Run();
