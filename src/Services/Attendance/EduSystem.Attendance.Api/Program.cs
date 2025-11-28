using EduSystem.Attendance.Api.DependencyResolver;
using EduSystem.Attendance.Application.DependencyResolver;
using EduSystem.Attendance.Infrastructure.DependencyResolver;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.Shared.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMultiTenancy();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services
    .AddSwaggerConfiguration(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMultiTenancy();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");
app.Run();
