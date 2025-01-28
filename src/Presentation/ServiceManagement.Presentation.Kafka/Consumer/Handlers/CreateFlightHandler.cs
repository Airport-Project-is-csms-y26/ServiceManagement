using ServiceManagement.Application.Contracts.Tasks;
using ServiceManagement.Application.Contracts.Tasks.Operations;
using ServiceManagement.Application.Models.Tasks;
using Tasks.Kafka.Contracts;
using Task = System.Threading.Tasks.Task;

namespace ServiceManagement.Presentation.Kafka.Consumer.Handlers;

public class CreateFlightHandler : IKafkaMessageHandler<FlightCreationKey, FlightCreationValue>
{
    private readonly ITaskService _taskService;

    public CreateFlightHandler(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task HandleAsync(
        IEnumerable<IKafkaConsumerMessage<FlightCreationKey, FlightCreationValue>> messages,
        CancellationToken cancellationToken)
    {
        foreach (IKafkaConsumerMessage<FlightCreationKey, FlightCreationValue> message in messages)
        {
            FlightCreationValue value = message.Value;
            var request = new CreateTaskOperation.Request(
                value.FlightId,
                value.PlaneNumber,
                TaskType.TechnicalInspection,
                "any",
                value.DepartTime.ToDateTimeOffset().AddDays(-1));
            await _taskService.Create(request, cancellationToken);
        }
    }
}