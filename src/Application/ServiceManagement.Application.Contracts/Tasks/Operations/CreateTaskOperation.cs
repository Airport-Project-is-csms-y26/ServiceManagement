using ServiceManagement.Application.Models.Tasks;

namespace ServiceManagement.Application.Contracts.Tasks.Operations;

public static class CreateTaskOperation
{
    public readonly record struct Request(
        long FlightId,
        long PlaneNumber,
        TaskType Type,
        string Executor,
        DateTimeOffset StartTime);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record InvalidFlightId : Result;

        public sealed record InvalidPlaneNumber : Result;

        public sealed record InvalidStartTime : Result;
    }
}