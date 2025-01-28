using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ServiceManagement.Application.Contracts.Tasks;
using ServiceManagement.Application.Contracts.Tasks.Operations;
using ServiceManagement.Presentation.Grpc.Util;
using System.Diagnostics;
using Tasks.TaskService.Contracts;
using GetTasksRequest = Tasks.TaskService.Contracts.GetTasksRequest;
using Task = ServiceManagement.Application.Models.Tasks.Task;

namespace ServiceManagement.Presentation.Grpc.Controllers;

public class TaskController : TaskService.TaskServiceBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public override async Task<CreateTaskResponse> CreateTask(CreateTaskRequest request, ServerCallContext context)
    {
        CreateTaskOperation.Result result = await _taskService.Create(
            new CreateTaskOperation.Request(
                request.FlightId,
                request.PlaneNumber,
                TaskTypeMapper.ToTaskType(request.Type),
                request.Executor,
                request.StartTime.ToDateTimeOffset()),
            context.CancellationToken);

        return result switch
        {
            CreateTaskOperation.Result.InvalidFlightId invalidFlightId => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Invalid flight number")),

            CreateTaskOperation.Result.InvalidPlaneNumber invalidPlaneNumber => throw new RpcException(new Status(
                StatusCode.NotFound,
                "Negative plane number")),

            CreateTaskOperation.Result.InvalidStartTime invalidStartTime => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Start time in the past")),

            CreateTaskOperation.Result.Success success => new CreateTaskResponse(),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<UpdateTaskStateResponse> UpdateTaskState(
        UpdateTaskStateRequest request,
        ServerCallContext context)
    {
        ChangeTaskStatusOperation.Result result =
            await _taskService.ChangeTaskStatus(
                new ChangeTaskStatusOperation.Request(request.TaskId, TaskStatusMapper.ToTaskStatus(request.NewState)),
                context.CancellationToken);

        return result switch
        {
            ChangeTaskStatusOperation.Result.Success success => new UpdateTaskStateResponse(),

            ChangeTaskStatusOperation.Result.NotFound notFound => throw new RpcException(new Status(
                StatusCode.NotFound,
                "TaskNotFound")),

            ChangeTaskStatusOperation.Result.InvalidStatus invalidStatus => throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                $"Cannot change: current status is {invalidStatus.Status}!")),

            _ => throw new UnreachableException(),
        };
    }

    public override async Task<GetTasksResponse> GetTasks(GetTasksRequest request, ServerCallContext context)
    {
        var query = new Application.Contracts.Tasks.Operations.GetTasksRequest(
            request.PageSize,
            request.Cursor,
            request.Ids.ToArray());

        IAsyncEnumerable<Task> tasks = _taskService.GetTasks(query, context.CancellationToken);

        var reply = new GetTasksResponse();
        await foreach (Task task in tasks)
        {
            reply.Tasks.Add(new Tasks.TaskService.Contracts.Task
            {
                TaskId = task.TaskId,
                Executor = task.Executor,
                FlightId = task.FlightId,
                PlaneNumber = task.PlaneNumber,
                StartTime = Timestamp.FromDateTime(task.StartTime.UtcDateTime),
                State = TaskStatusMapper.ToProtoTaskState(task.State),
                Type = TaskTypeMapper.ToProtoTaskType(task.Type),
            });
        }

        return reply;
    }
}