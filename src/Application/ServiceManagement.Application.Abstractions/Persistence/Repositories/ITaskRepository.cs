using ServiceManagement.Application.Abstractions.Persistence.Queries;
using TaskStatus = ServiceManagement.Application.Models.Tasks.TaskStatus;

namespace ServiceManagement.Application.Abstractions.Persistence.Repositories;

public interface ITaskRepository
{
    Task Create(CreateTaskQuery query, CancellationToken cancellationToken);

    Task ChangeTaskStatus(long taskId, TaskStatus status, CancellationToken cancellationToken);

    Task<Models.Tasks.Task?> GetTaskById(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Application.Models.Tasks.Task> GetTaskByFlightId(
        long flightId,
        CancellationToken cancellationToken);

    IAsyncEnumerable<Models.Tasks.Task> GetTasks(GetTasksQuery query, CancellationToken cancellationToken);
}