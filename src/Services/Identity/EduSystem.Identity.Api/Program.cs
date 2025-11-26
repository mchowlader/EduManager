using Asp.Versioning.ApiExplorer;
using EduSystem.Identity.Api.DependencyResolver;
using EduSystem.Identity.Api.Endpoints;
using EduSystem.Identity.Application.Commands;
using EduSystem.Identity.Application.DependencyResolver;
using EduSystem.Identity.Application.IService;
using EduSystem.Identity.Domain.IRepository;
using EduSystem.Identity.Infrastructure.Contexts;
using EduSystem.Identity.Infrastructure.DependencyResolver;
using EduSystem.Identity.Infrastructure.Repositories;
using EduSystem.Identity.Infrastructure.Service;
using EduSystem.Identity.Infrastructure.Services;
using EduSystem.Shared.Infrastructure.Security;
using EduSystem.Shared.Messaging.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt",rollingInterval: RollingInterval.Day)
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
app.UseSwaggerConfiguration();
app.MapEndpoints();

// Program.cs
app.MapGet("/test-fail", () =>
{
    throw new Exception("This is a test exception!");
});

app.Run();
app.Run();
