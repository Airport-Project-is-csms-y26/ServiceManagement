using Microsoft.Extensions.DependencyInjection;
using ServiceManagement.Application.Contracts.Tasks;

namespace ServiceManagement.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<ITaskService, TaskService.MyTaskService>();
        return collection;
    }
}