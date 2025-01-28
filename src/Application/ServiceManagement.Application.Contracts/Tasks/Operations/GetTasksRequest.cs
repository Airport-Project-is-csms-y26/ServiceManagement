namespace ServiceManagement.Application.Contracts.Tasks.Operations;

public record GetTasksRequest(
    int PageSize,
    int Cursor,
    long[] Ids);