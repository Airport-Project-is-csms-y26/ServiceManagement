using ServiceManagement.Application.Contracts.Tasks.Operations;

namespace ServiceManagement.Application.Contracts.Tasks;

public interface ITaskService
{
    Task<CreateTaskOperation.Result> Create(CreateTaskOperation.Request request, CancellationToken cancellationToken);

    Task<ChangeTaskStatusOperation.Result> ChangeTaskStatus(ChangeTaskStatusOperation.Request request, CancellationToken cancellationToken);

    IAsyncEnumerable<Models.Tasks.Task> GetTasks(GetTasksRequest request, CancellationToken cancellationToken);
}