using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi;

namespace EduSystem.ApplicationUsers.Api.DependencyResolver;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //enable/disable runtime decide
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v1",
                Description = "Identity and Tenant Management API - Version 1"
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v2",
                Description = "Identity and Tenant Management API - Version 2"
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(
        this WebApplication app)
    {
        var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", false);

        if (!swaggerEnabled)
        {
            return app; 
        }

        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"EduSystem API {description.GroupName.ToUpperInvariant()}"
                );
            }
        });

        return app;
    }
}
