namespace ServiceManagement.Application.Models.Tasks;

public record Task(
    long TaskId,
    long FlightId,
    long PlaneNumber,
    TaskType Type,
    TaskStatus State,
    string Executor,
    DateTimeOffset StartTime);