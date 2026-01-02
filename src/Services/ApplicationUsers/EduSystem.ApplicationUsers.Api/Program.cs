using System.IdentityModel.Tokens.Jwt;
using EduSystem.ApplicationUsers.Api.DependencyResolver;
using EduSystem.ApplicationUsers.Application.DependencyResolver;
using EduSystem.ApplicationUsers.Infrastructure.DependencyResolver;
using EduSystem.Shared.Infrastructure.Extensions;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using EduSystem.ApplicationUsers.Api.Endpoints;
using Serilog;
using EduSystem.ApplicationUsers.Infrastructure.Contexts;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // To avoid claim type mapping issues

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
app.MapMigrationEndpoints();

// Startup Migrations for all tenants
using (var scope = app.Services.CreateScope())
{
    var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
    var migrationService = scope.ServiceProvider.GetRequiredService<ITenantMigrationService<AppUserDbContext>>();

    var tenants = await tenantProvider.GetActiveTenantsAsync();
    await migrationService.MigrateAllTenantsAsync(tenants);
}

app.Run();
