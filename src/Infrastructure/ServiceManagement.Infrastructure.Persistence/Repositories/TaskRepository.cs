using Npgsql;
using ServiceManagement.Application.Abstractions.Persistence.Queries;
using ServiceManagement.Application.Abstractions.Persistence.Repositories;
using ServiceManagement.Application.Models.Tasks;
using System.Runtime.CompilerServices;
using Task = System.Threading.Tasks.Task;
using TaskStatus = ServiceManagement.Application.Models.Tasks.TaskStatus;

namespace ServiceManagement.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly NpgsqlDataSource _npgsqlDataSource;

    public TaskRepository(NpgsqlDataSource dataSource)
    {
        _npgsqlDataSource = dataSource;
    }

    public async Task Create(CreateTaskQuery query, CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO tasks (task_flight_id, task_plane_number, task_type, task_status, task_executor, task_start_time)
                           VALUES (@FlightId, @PlaneNumber, @Type, 'planned', @Executor, @StartTime)
                           RETURNING task_id;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("FlightId", query.FlightId),
                new NpgsqlParameter("PlaneNumber", query.PlaneNumber),
                new NpgsqlParameter("Executor", query.Executor),
                new NpgsqlParameter("Type", query.Type),
                new NpgsqlParameter("StartTime", query.StartTime),
            },
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ChangeTaskStatus(long taskId, TaskStatus status, CancellationToken cancellationToken)
    {
        const string sql = """
                           UPDATE tasks
                           SET task_status = @State
                           WHERE task_id = @TaskId;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("State", status),
                new NpgsqlParameter("TaskId", taskId),
            },
        };
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Application.Models.Tasks.Task?> GetTaskById(long id, CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT task_id,
                                  task_flight_id,
                                  task_plane_number,
                                  task_type,
                                  task_status,
                                  task_executor,
                                  task_start_time
                           FROM tasks
                           WHERE task_id = @Id;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("Id", id),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Application.Models.Tasks.Task(
            reader.GetInt64(reader.GetOrdinal("task_id")),
            reader.GetInt64(reader.GetOrdinal("task_flight_id")),
            reader.GetInt64(reader.GetOrdinal("task_plane_number")),
            reader.GetFieldValue<TaskType>(reader.GetOrdinal("task_type")),
            reader.GetFieldValue<TaskStatus>(reader.GetOrdinal("task_status")),
            reader.GetString(reader.GetOrdinal("task_executor")),
            reader.GetDateTime(reader.GetOrdinal("task_start_time")));
    }

    public async IAsyncEnumerable<Application.Models.Tasks.Task> GetTaskByFlightId(
        long flightId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT task_id,
                                  task_flight_id,
                                  task_plane_number,
                                  task_type,
                                  task_status,
                                  task_executor,
                                  task_start_time
                           FROM tasks
                           WHERE task_flight_id = @FlightId;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("FlightId", flightId),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Application.Models.Tasks.Task(
                reader.GetInt64(reader.GetOrdinal("task_id")),
                reader.GetInt64(reader.GetOrdinal("task_flight_id")),
                reader.GetInt64(reader.GetOrdinal("task_plane_number")),
                reader.GetFieldValue<TaskType>(reader.GetOrdinal("task_type")),
                reader.GetFieldValue<TaskStatus>(reader.GetOrdinal("task_status")),
                reader.GetString(reader.GetOrdinal("task_executor")),
                reader.GetDateTime(reader.GetOrdinal("task_start_time")));
        }
    }

    public async IAsyncEnumerable<Application.Models.Tasks.Task> GetTasks(
        GetTasksQuery query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT task_id,
                                  task_flight_id,
                                  task_plane_number,
                                  task_type,
                                  task_status,
                                  task_executor,
                                  task_start_time
                           FROM tasks
                           WHERE (task_id > @Cursor)
                           AND (cardinality(@Ids) = 0 OR task_id = any(@Ids))
                           LIMIT @Limit;
                           """;

        await using NpgsqlConnection connection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection)
        {
            Parameters =
            {
                new NpgsqlParameter("Cursor", query.Cursor),
                new NpgsqlParameter("Limit", query.PageSize),
                new NpgsqlParameter("Ids", query.Ids),
            },
        };

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Application.Models.Tasks.Task(
                reader.GetInt64(reader.GetOrdinal("task_id")),
                reader.GetInt64(reader.GetOrdinal("task_flight_id")),
                reader.GetInt64(reader.GetOrdinal("task_plane_number")),
                reader.GetFieldValue<TaskType>(reader.GetOrdinal("task_type")),
                reader.GetFieldValue<TaskStatus>(reader.GetOrdinal("task_status")),
                reader.GetString(reader.GetOrdinal("task_executor")),
                reader.GetDateTime(reader.GetOrdinal("task_start_time")));
        }
    }
}