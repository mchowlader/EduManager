using System.Net;
using System.Reflection;

namespace EduSystem.ApplicationUsers.Api.EndPoints
{
    public static class EndpointExtensions
    {
        public static IEndpointRouteBuilder MapEndPoints(this IEndpointRouteBuilder app)
        {
            var endPointType = "hello";
            return default;
        }


        public static IEnumerable<Type> GetEndPointType()
        {
            var assembly = Assembly.GetExecutingAssembly();

            return assembly
                .GetTypes()
                .Where(t => t.IsClass 
                    && !t.IsAbstract
                    && t.GetInterfaces().Any(i => i == typeof(IEndpoints)))
                .OrderBy(t => t.Name);
        }
    }

}
