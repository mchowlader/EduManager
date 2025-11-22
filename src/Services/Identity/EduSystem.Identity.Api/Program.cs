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
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduSystem Tenant API v1");
    c.RoutePrefix = "swagger";
});
app.MapEndpoints();
app.Run();
