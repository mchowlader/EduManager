using EduSystem.ApplicationUsers.Api.DependencyResolver;
using EduSystem.ApplicationUsers.Api.EndPoints;
using EduSystem.ApplicationUsers.Application.DependencyResolver;
using EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;
using Serilog;
using EduSystem.Shared.Infrastructure.MultiTenancy;

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
app.UseSwaggerConfiguration();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.UseMultiTenancy();
app.UseAuthorization();
app.MapControllers();
app.MapEndpoints();
app.Run();
