using ServiceManagement.Application.Models.Tasks;

namespace ServiceManagement.Application.Abstractions.Persistence.Queries;

public record CreateTaskQuery(
    long FlightId,
    long PlaneNumber,
    TaskType Type,
    string Executor,
    DateTimeOffset StartTime);