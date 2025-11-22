using EduSystem.ApplicationUsers.Api.DependencyResolver;
using EduSystem.ApplicationUsers.Application.DependencyResolver;
using EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduSystem Application Users API v1");
    c.RoutePrefix = "swagger";
});
// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseHttpsRedirection();

app.Run();