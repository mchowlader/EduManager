using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EduSystem.Identity.Api.DependencyResolver;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // Version 1
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v1",
                Description = "API Version 1"
            });

            // Version 2
            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "EduSystem Identity API",
                Version = "v2",
                Description = "API Version 2"
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

            // ✅ Deprecated warning শুধু description এ যোগ করবে
            options.OperationFilter<DeprecatedOperationFilter>();

            // JWT Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your-token}"
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

// ✅ Deprecated warning শুধু description/operation এ দেখাবে
public class DeprecatedOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the route contains v1 and belongs to Tenants
        var relativePath = context.ApiDescription.RelativePath;

        // শুধুমাত্র Tenant v1 endpoints এ deprecated mark করুন
        if (relativePath != null &&
            relativePath.Contains("/v1/tenants"))
        {

            // ⚠️ Description এ deprecation notice যোগ করুন (endpoint expand করলে দেখাবে)
            var deprecationNotice = "⚠️ **Deprecation Notice:** This endpoint is deprecated and will be removed in a future release. Please migrate to v2 API.";

            operation.Description = string.IsNullOrEmpty(operation.Description)
                ? deprecationNotice
                : $"{deprecationNotice}\n\n{operation.Description}";
        }
    }
}
