using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using PassengerService.Infrastructure.Persistence.Options;
using ServiceManagement.Application.Abstractions.Persistence.Repositories;
using ServiceManagement.Application.Models.Tasks;
using ServiceManagement.Infrastructure.Persistence.Migrations;
using ServiceManagement.Infrastructure.Persistence.Repositories;
using TaskStatus = ServiceManagement.Application.Models.Tasks.TaskStatus;

namespace ServiceManagement.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMigration(
        this IServiceCollection collection)
    {
        collection
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(serviceProvider =>
                    serviceProvider
                        .GetRequiredService<IOptions<PostgresOptions>>()
                        .Value
                        .GetConnectionString())
                .ScanIn(typeof(InitialMigration).Assembly));

        collection.AddSingleton<NpgsqlDataSource>(provider =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(provider
                .GetRequiredService<IOptions<PostgresOptions>>()
                .Value
                .GetConnectionString());

            dataSourceBuilder.MapEnum<TaskStatus>(pgName: "task_state");
            dataSourceBuilder.MapEnum<TaskType>(pgName: "task_type");

            return dataSourceBuilder.Build();
        });

        return collection;
    }

    public static IServiceCollection AddInfrastructureDataAccess(this IServiceCollection collection)
    {
        collection.AddScoped<ITaskRepository, TaskRepository>();

        return collection;
    }
}