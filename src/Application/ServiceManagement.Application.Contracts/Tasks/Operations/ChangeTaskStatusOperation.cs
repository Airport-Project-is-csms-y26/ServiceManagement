using TaskStatus = ServiceManagement.Application.Models.Tasks.TaskStatus;

namespace ServiceManagement.Application.Contracts.Tasks.Operations;

public class ChangeTaskStatusOperation
{
    public readonly record struct Request(long Id, TaskStatus Status);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success : Result;

        public sealed record NotFound : Result;

        public sealed record InvalidStatus(TaskStatus Status) : Result;
    }
}