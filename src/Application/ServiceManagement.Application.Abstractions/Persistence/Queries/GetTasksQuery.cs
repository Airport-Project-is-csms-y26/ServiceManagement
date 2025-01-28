namespace ServiceManagement.Application.Abstractions.Persistence.Queries;

public record GetTasksQuery(
    int PageSize,
    int Cursor,
    long[] Ids);