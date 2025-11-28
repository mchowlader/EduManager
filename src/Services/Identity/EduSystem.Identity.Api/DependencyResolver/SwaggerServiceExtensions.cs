using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace EduSystem.Identity.Api.DependencyResolver;

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

            // Add JWT Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(
        this WebApplication app)
    {
        var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", true);

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
