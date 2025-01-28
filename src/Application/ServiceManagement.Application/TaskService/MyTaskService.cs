using Flights.FlightsService.Contracts;
using Google.Protobuf.WellKnownTypes;
using ServiceManagement.Application.Abstractions.Persistence.Queries;
using ServiceManagement.Application.Abstractions.Persistence.Repositories;
using ServiceManagement.Application.Contracts.Tasks;
using ServiceManagement.Application.Contracts.Tasks.Operations;
using ServiceManagement.Presentation.FlightGrpcClient.Clients.Interfaces;
using ServiceManagement.Presentation.Kafka.Producer;
using Tasks.Kafka.Contracts;
using Task = ServiceManagement.Application.Models.Tasks.Task;
using TaskStatus = ServiceManagement.Application.Models.Tasks.TaskStatus;

namespace ServiceManagement.Application.TaskService;

public class MyTaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IKafkaProducer<TaskProcessingKey, TaskProcessingValue> _producer;
    private readonly IFlightClient _flightClient;

    public MyTaskService(
        ITaskRepository taskRepository,
        IKafkaProducer<TaskProcessingKey, TaskProcessingValue> producer,
        IFlightClient flightClient)
    {
        _taskRepository = taskRepository;
        _producer = producer;
        _flightClient = flightClient;
    }

    public async Task<CreateTaskOperation.Result> Create(
        CreateTaskOperation.Request request,
        CancellationToken cancellationToken)
    {
        Flight[] flights = await _flightClient
            .GetFlights(int.MaxValue, 0, new[] { request.FlightId }, cancellationToken)
            .ToArrayAsync(cancellationToken);
        if (flights.Length == 0)
        {
            return new CreateTaskOperation.Result.InvalidFlightId();
        }

        if (flights[0].PlaneNumber != request.PlaneNumber)
        {
            return new CreateTaskOperation.Result.InvalidPlaneNumber();
        }

        if (request.StartTime >= flights[0].DepartureTime.ToDateTimeOffset() || request.StartTime < DateTimeOffset.Now.Date)
        {
            return new CreateTaskOperation.Result.InvalidStartTime();
        }

        var createTaskQuery = new CreateTaskQuery(
            request.FlightId,
            request.PlaneNumber,
            request.Type,
            request.Executor,
            request.StartTime);

        await _taskRepository.Create(createTaskQuery, cancellationToken);

        return new CreateTaskOperation.Result.Success();
    }

    public async Task<ChangeTaskStatusOperation.Result> ChangeTaskStatus(
        ChangeTaskStatusOperation.Request request,
        CancellationToken cancellationToken)
    {
        Task? task = await _taskRepository.GetTaskById(request.Id, cancellationToken);

        if (task == null)
        {
            return new ChangeTaskStatusOperation.Result.NotFound();
        }

        if (request.Status < task.State)
        {
            return new ChangeTaskStatusOperation.Result.InvalidStatus(task.State);
        }

        await _taskRepository.ChangeTaskStatus(request.Id, request.Status, cancellationToken);

        if (request.Status == TaskStatus.Completed || request.Status == TaskStatus.Cancelled)
        {
            await CheckAllDone(task.FlightId, cancellationToken);
        }

        return new ChangeTaskStatusOperation.Result.Success();
    }

    public IAsyncEnumerable<Task> GetTasks(GetTasksRequest request, CancellationToken cancellationToken)
    {
        var query = new GetTasksQuery(
            request.PageSize,
            request.Cursor,
            request.Ids);

        return _taskRepository.GetTasks(query, cancellationToken);
    }

    private async System.Threading.Tasks.Task CheckAllDone(long flightId, CancellationToken cancellationToken)
    {
        bool flag = true;
        await foreach (Task task in _taskRepository.GetTaskByFlightId(flightId, cancellationToken))
        {
            if (task.State == TaskStatus.Planned || task.State == TaskStatus.Running)
            {
                flag = false;
                break;
            }
        }

        if (flag)
        {
            var key = new TaskProcessingKey { FlightId = flightId };
            var value = new TaskProcessingValue
            {
                FlightId = flightId,
                DoneAt = Timestamp.FromDateTimeOffset(DateTimeOffset.Now),
            };
            Console.WriteLine(flightId);

            await _producer.ProduceAsync(key, value, cancellationToken);
        }
    }
}