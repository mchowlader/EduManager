using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EduSystem.ApplicationUsers.Api.DependencyResolver;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName);

            // Version 1
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduSystem ApplicationUsers API",
                Version = "v1",
                Description = "ApplicationUsers and Tenant Management API - Version 1"
            });

            // ✅ এই filter টা নিশ্চিত করে যে প্রতিটি version এর জন্য শুধুমাত্র সেই version এর APIs দেখাবে
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (apiDesc.RelativePath == null)
                    return false;

                // Route থেকে version extract করুন (e.g., "api/v1/tenants" or "api/v2/tenants")
                var pathSegments = apiDesc.RelativePath.Split('/');

                // Find the version segment (should be like "v1" or "v2")
                var versionSegment = pathSegments.FirstOrDefault(s => s.StartsWith("v") && char.IsDigit(s.LastOrDefault()));

                if (string.IsNullOrEmpty(versionSegment))
                    return false; // Version না থাকলে কোনো document এ দেখাবে না

                // Match with document name - শুধুমাত্র matching version দেখাবে
                return versionSegment == docName;
            });

            // JWT Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
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
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
