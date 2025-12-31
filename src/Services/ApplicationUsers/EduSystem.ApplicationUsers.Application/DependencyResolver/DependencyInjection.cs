using EduSystem.ApplicationUsers.Domain.IRepository;
using EduSystem.ApplicationUsers.Application.Features.Base.Commands;
using EduSystem.ApplicationUsers.Application.Features.Base.Queries;
using EduSystem.ApplicationUsers.Domain.Entities;
using EduSystem.ApplicationUsers.Shared.Common;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSystem.ApplicationUsers.Application.DependencyResolver;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Automated registration for generic handlers using Reflection
        RegisterGenericHandlers(services);

        return services;
    }

    private static void RegisterGenericHandlers(IServiceCollection services)
    {
        var domainAssembly = typeof(BaseEntity).Assembly;
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        var entityTypes = domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseEntity)))
            .ToList();

        foreach (var entityType in entityTypes)
        {
            var entityName = entityType.Name;

            // Find corresponding DTOs by naming convention
            var createDtoType = applicationAssembly.GetType($"EduSystem.ApplicationUsers.Application.DTOs.{entityName}CreateDto");
            var updateDtoType = applicationAssembly.GetType($"EduSystem.ApplicationUsers.Application.DTOs.{entityName}UpdateDto");
            var responseDtoType = applicationAssembly.GetType($"EduSystem.ApplicationUsers.Application.DTOs.{entityName}ResponseDto");

            // 1. Register Delete Handler (Only needs Entity)
            services.AddTransient(
                typeof(IRequestHandler<,>).MakeGenericType(
                    typeof(BaseDeleteEntityCommand<>).MakeGenericType(entityType),
                    typeof(Result)),
                typeof(BaseDeleteEntityCommandHandler<>).MakeGenericType(entityType));

            // 2. Register Create, Update, GetAll, GetById Handlers (Need Entity and DTOs)
            if (createDtoType != null && responseDtoType != null)
            {
                // Create
                services.AddTransient(
                    typeof(IRequestHandler<,>).MakeGenericType(
                        typeof(BaseCreateEntityCommand<,,>).MakeGenericType(entityType, createDtoType, responseDtoType),
                        typeof(Result<>).MakeGenericType(responseDtoType)),
                    typeof(BaseCreateEntityCommandHandler<,,>).MakeGenericType(entityType, createDtoType, responseDtoType));

                // GetAll
                services.AddTransient(
                    typeof(IRequestHandler<,>).MakeGenericType(
                        typeof(BaseGetAllEntitiesQuery<,>).MakeGenericType(entityType, responseDtoType),
                        typeof(Result<>).MakeGenericType(typeof(PagedList<>).MakeGenericType(responseDtoType))),
                    typeof(BaseGetAllEntitiesQueryHandler<,>).MakeGenericType(entityType, responseDtoType));

                // GetById
                services.AddTransient(
                    typeof(IRequestHandler<,>).MakeGenericType(
                        typeof(BaseGetEntityByIdQuery<,>).MakeGenericType(entityType, responseDtoType),
                        typeof(Result<>).MakeGenericType(responseDtoType)),
                    typeof(BaseGetEntityByIdQueryHandler<,>).MakeGenericType(entityType, responseDtoType));
            }

            if (updateDtoType != null && responseDtoType != null)
            {
                // Update
                services.AddTransient(
                    typeof(IRequestHandler<,>).MakeGenericType(
                        typeof(BaseUpdateEntityCommand<,,>).MakeGenericType(entityType, updateDtoType, responseDtoType),
                        typeof(Result<>).MakeGenericType(responseDtoType)),
                    typeof(BaseUpdateEntityCommandHandler<,,>).MakeGenericType(entityType, updateDtoType, responseDtoType));
            }
        }
    }
}
